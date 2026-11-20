using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Users;

public sealed class UserEndpointsTests(SqlServerFixture fixture)
{
    private const string UserPassword = "a-long-enough-password";

    private static Uri UsersUrl(string path = "") => new(ApiEndpoints.Organization.Users.GetAll + path, UriKind.Relative);

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static async Task<(Guid OwnCompanyId, Guid AdministratorRoleId)> GetReferencesAsync(HttpClient client)
    {
        var companies = await client.GetFromJsonAsync<OwnCompanyResponse[]>(new Uri(ApiEndpoints.Organization.OwnCompanies.GetAll, UriKind.Relative), Token);
        var roles = await client.GetFromJsonAsync<RoleResponse[]>(new Uri(ApiEndpoints.Organization.Roles.GetAll, UriKind.Relative), Token);
        return (companies!.Single().Id, roles!.Single(x => x.Name == RoleNames.Administrator).Id);
    }

    private static async Task<Guid> CreateRoleAsync(HttpClient client, string name)
    {
        using var response = await client.PostAsJsonAsync(new Uri(ApiEndpoints.Organization.Roles.Create, UriKind.Relative), new EditRoleRequest(name, "Description"), Token);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RoleResponse>(Token))!.Id;
    }

    private static CreateUserRequest CreateRequest(Guid ownCompanyId, Guid roleId, string username = "nurse@example.com") =>
        new("Anna", null, "Svensson", username, ownCompanyId, roleId, AuthProvider.Local, UserPassword);

    [Fact]
    public async Task GetAllWithoutTokenReturnsUnauthorized()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);

        // Act
        using var response = await host.Client.GetAsync(UsersUrl(), Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllWithGarbageTokenReturnsUnauthorized()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-jwt");

        // Act
        using var response = await host.Client.GetAsync(UsersUrl(), Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllAsAdministratorReturnsBootstrapAdminWithoutCredentials()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);

        // Act
        using var response = await host.Client.GetAsync(UsersUrl(), Token);
        var body = await response.Content.ReadAsStringAsync(Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldContain(ApiTestHost.AdminUsername);
        body.ShouldNotContain("passwordHash", Case.Insensitive);
        body.ShouldNotContain("securityStamp", Case.Insensitive);
    }

    [Fact]
    public async Task CreateLocalUserAsAdministratorAllowsThatUserToSignIn()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);
        var (ownCompanyId, _) = await GetReferencesAsync(host.Client);
        var nurseRoleId = await CreateRoleAsync(host.Client, "Nurse");

        // Act
        using var created = await host.Client.PostAsJsonAsync(UsersUrl(), CreateRequest(ownCompanyId, nurseRoleId), Token);
        using var login = await host.Client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Auth.Login, UriKind.Relative),
            new LoginRequest("nurse@example.com", UserPassword),
            Token);

        // Assert
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task NonAdministratorIsForbiddenFromManagingUsers()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);
        var (ownCompanyId, _) = await GetReferencesAsync(host.Client);
        var nurseRoleId = await CreateRoleAsync(host.Client, "Nurse");
        (await host.Client.PostAsJsonAsync(UsersUrl(), CreateRequest(ownCompanyId, nurseRoleId), Token)).Dispose();
        using var login = await host.Client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Auth.Login, UriKind.Relative),
            new LoginRequest("nurse@example.com", UserPassword),
            Token);
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>(Token))!.AccessToken;
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        using var response = await host.Client.GetAsync(UsersUrl(), Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateWithDuplicateUsernameReturnsConflict()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);
        var (ownCompanyId, adminRoleId) = await GetReferencesAsync(host.Client);

        // Act
        using var response = await host.Client.PostAsJsonAsync(UsersUrl(), CreateRequest(ownCompanyId, adminRoleId, ApiTestHost.AdminUsername), Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateWithShortPasswordReturnsValidationProblem()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);
        var (ownCompanyId, adminRoleId) = await GetReferencesAsync(host.Client);
        var request = CreateRequest(ownCompanyId, adminRoleId) with { Password = "short" };

        // Act
        using var response = await host.Client.PostAsJsonAsync(UsersUrl(), request, Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateEntraUserWhenEntraIsNotConfiguredReturnsServiceUnavailable()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);
        var (ownCompanyId, adminRoleId) = await GetReferencesAsync(host.Client);
        var request = CreateRequest(ownCompanyId, adminRoleId, "bo@example.com") with { AuthProvider = AuthProvider.Entra, Password = null };

        // Act
        using var response = await host.Client.PostAsJsonAsync(UsersUrl(), request, Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task DeleteOwnAccountReturnsConflict()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);
        var users = await host.Client.GetFromJsonAsync<UserResponse[]>(UsersUrl(), Token);
        var adminId = users!.Single().Id;

        // Act
        using var response = await host.Client.DeleteAsync(UsersUrl("/" + adminId), Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteOtherUserRemovesThem()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);
        var (ownCompanyId, adminRoleId) = await GetReferencesAsync(host.Client);
        using var created = await host.Client.PostAsJsonAsync(UsersUrl(), CreateRequest(ownCompanyId, adminRoleId), Token);
        var user = await created.Content.ReadFromJsonAsync<UserResponse>(Token);

        // Act
        using var deleted = await host.Client.DeleteAsync(UsersUrl("/" + user!.Id), Token);
        using var fetched = await host.Client.GetAsync(UsersUrl("/" + user.Id), Token);

        // Assert
        deleted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        fetched.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateKeepsUsernameAndReturnsNewNames()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);
        var (ownCompanyId, adminRoleId) = await GetReferencesAsync(host.Client);
        using var created = await host.Client.PostAsJsonAsync(UsersUrl(), CreateRequest(ownCompanyId, adminRoleId), Token);
        var user = await created.Content.ReadFromJsonAsync<UserResponse>(Token);
        var request = new EditUserRequest("Bo", null, "Nilsson", ownCompanyId, adminRoleId);

        // Act
        using var response = await host.Client.PutAsJsonAsync(UsersUrl("/" + user!.Id), request, Token);
        var updated = await response.Content.ReadFromJsonAsync<UserResponse>(Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated!.FirstName.ShouldBe("Bo");
        updated.Username.ShouldBe("nurse@example.com");
    }
}
