using System.Net;
using System.Net.Http.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Extensions;

public sealed class PermissionAccessTests(SqlServerFixture fixture)
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static Uri Url(string path) => new(path, UriKind.Relative);

    private static string WithId(string route, Guid id) =>
        route.Replace("{id:guid}", id.ToString("D"), StringComparison.Ordinal);

    private static async Task<RoleResponse> CreateRoleAsync(ApiTestHost host, string name, params string[] permissions)
    {
        using var response = await host.Client.PostAsJsonAsync(
            Url(ApiEndpoints.Organization.Roles.Create), new EditRoleRequest(name, name, permissions), Token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RoleResponse>(Token))!;
    }

    [Fact]
    public async Task ARoleWithNoRightsSeesNothingButItsOwnProfile()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, Token);
        await host.SignInAsNewNonAdminAsync(Token, []);

        // Act
        using var companies = await host.Client.GetAsync(Url(ApiEndpoints.Organization.OwnCompanies.GetAll), Token);
        using var contacts = await host.Client.GetAsync(Url(ApiEndpoints.Contact.Addresses.GetAll), Token);
        var profile = await host.Client.GetFromJsonAsync<MyProfileResponse>(Url(ApiEndpoints.Organization.Users.Me), Token);

        // Assert
        companies.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        contacts.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        profile.ShouldNotBeNull().IsAdministrator.ShouldBeFalse();
        profile.Permissions.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task AChangedRoleAppliesToATokenThatIsAlreadyIssued()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, Token);
        await host.SignInAsNewNonAdminAsync(Token, []);
        var token = host.Client.DefaultRequestHeaders.Authorization;
        using var before = await host.Client.PostAsJsonAsync(
            Url(ApiEndpoints.Organization.WorkGroups.Create), new EditWorkGroupRequest("Ward 1", "Description"), Token);

        await host.SignInAsAdminAsync(Token);
        var role = (await host.Client.GetFromJsonAsync<RoleResponse[]>(Url(ApiEndpoints.Organization.Roles.GetAll), Token))!
            .Single(x => x.Name == "Nurse");
        using var granted = await host.Client.PutAsJsonAsync(
            Url(WithId(ApiEndpoints.Organization.Roles.Update, role.Id)),
            new EditRoleRequest(role.Name, role.Description, [Permissions.WorkGroups.Create]),
            Token);
        host.Client.DefaultRequestHeaders.Authorization = token;

        // Act
        using var after = await host.Client.PostAsJsonAsync(
            Url(ApiEndpoints.Organization.WorkGroups.Create), new EditWorkGroupRequest("Ward 2", "Description"), Token);

        // Assert
        before.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        granted.IsSuccessStatusCode.ShouldBeTrue();
        after.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task SomeoneWhoManagesUsersCannotUseThatToGainOrHandOutMore()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, Token);
        var company = (await host.Client.GetFromJsonAsync<OwnCompanyResponse[]>(
            Url(ApiEndpoints.Organization.OwnCompanies.GetAll), Token))!.Single();
        var porter = await CreateRoleAsync(host, "Porter");
        var chief = await CreateRoleAsync(host, "Chief", Permissions.WorkGroups.Delete);
        var roles = (await host.Client.GetFromJsonAsync<RoleResponse[]>(Url(ApiEndpoints.Organization.Roles.GetAll), Token))!;
        var administratorRole = roles.Single(x => x.Name == RoleNames.Administrator);
        var administrator = (await host.Client.GetFromJsonAsync<UserResponse[]>(Url(ApiEndpoints.Organization.Users.GetAll), Token))!.Single();
        await host.SignInAsNewNonAdminAsync(Token, [Permissions.Users.Create, Permissions.Users.Delete]);

        CreateUserRequest NewUser(string username, Guid roleId) =>
            new("Test", null, "Person", username, company.Id, roleId, AuthProvider.Local, "a-long-enough-password");

        // Act
        using var allowed = await host.Client.PostAsJsonAsync(Url(ApiEndpoints.Organization.Users.Create), NewUser("porter@example.com", porter.Id), Token);
        using var tooMuch = await host.Client.PostAsJsonAsync(Url(ApiEndpoints.Organization.Users.Create), NewUser("chief@example.com", chief.Id), Token);
        using var admin = await host.Client.PostAsJsonAsync(Url(ApiEndpoints.Organization.Users.Create), NewUser("boss@example.com", administratorRole.Id), Token);
        using var delete = await host.Client.DeleteAsync(Url(WithId(ApiEndpoints.Organization.Users.Delete, administrator.Id)), Token);
        using var rights = await host.Client.PostAsJsonAsync(Url(ApiEndpoints.Organization.Roles.Create), new EditRoleRequest("Sneaky", "Sneaky"), Token);

        // Assert
        allowed.StatusCode.ShouldBe(HttpStatusCode.Created);
        tooMuch.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        admin.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await admin.Content.ReadAsStringAsync(Token)).ShouldContain("Only administrators can give someone the Administrator role.");
        delete.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        rights.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
