using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.AccessRequests;

public sealed class AccessRequestEndpointsTests(SqlServerFixture fixture)
{
    private const string NursePassword = "a-long-enough-password";

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static Uri Url(string path) => new(path, UriKind.Relative);

    private static Uri ForId(string template, Guid id) => Url(template.Replace("{id:guid}", id.ToString("D"), StringComparison.Ordinal));

    private static async Task SignInAsNurseAsync(ApiTestHost host)
    {
        var companies = await host.Client.GetFromJsonAsync<OwnCompanyResponse[]>(Url(ApiEndpoints.Organization.OwnCompanies.GetAll), Token);
        using var roleResponse = await host.Client.PostAsJsonAsync(Url(ApiEndpoints.Organization.Roles.Create), new EditRoleRequest("Nurse", "Nurse"), Token);
        var role = await roleResponse.Content.ReadFromJsonAsync<RoleResponse>(Token);

        using var created = await host.Client.PostAsJsonAsync(
            Url(ApiEndpoints.Organization.Users.Create),
            new CreateUserRequest("Nina", null, "Nurse", "nurse@example.com", companies!.Single().Id, role!.Id, AuthProvider.Local, NursePassword),
            Token);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);

        using var login = await host.Client.PostAsJsonAsync(
            Url(ApiEndpoints.Auth.Login),
            new LoginRequest("nurse@example.com", NursePassword),
            Token);
        var token = await login.Content.ReadFromJsonAsync<LoginResponse>(Token);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
    }

    [Theory]
    [InlineData("GET", ApiEndpoints.Organization.AccessRequests.GetAll)]
    [InlineData("GET", ApiEndpoints.Organization.AccessRequests.Me)]
    [InlineData("POST", ApiEndpoints.Organization.AccessRequests.Me)]
    public async Task EveryEndpointRequiresASignedInCaller(string method, string path)
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        using var request = new HttpRequestMessage(new HttpMethod(method), Url(path));

        // Act
        using var response = await host.Client.SendAsync(request, Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ApprovalEndpointsRejectCallersWithoutATokenBeforeTouchingData()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);

        // Act
        using var approve = await host.Client.PostAsJsonAsync(
            ForId(ApiEndpoints.Organization.AccessRequests.Approve, Guid.NewGuid()),
            new ApproveAccessRequestRequest(Guid.NewGuid(), Guid.NewGuid(), "Anna", null, "Svensson", false),
            Token);
        using var reject = await host.Client.PostAsJsonAsync(
            ForId(ApiEndpoints.Organization.AccessRequests.Reject, Guid.NewGuid()),
            new RejectAccessRequestRequest(null),
            Token);
        using var reopen = await host.Client.PostAsync(ForId(ApiEndpoints.Organization.AccessRequests.Reopen, Guid.NewGuid()), null, Token);

        // Assert
        approve.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        reject.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        reopen.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ANonAdministratorCannotListOrDecideRequests()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);
        await SignInAsNurseAsync(host);

        // Act
        using var list = await host.Client.GetAsync(Url(ApiEndpoints.Organization.AccessRequests.GetAll), Token);
        using var approve = await host.Client.PostAsJsonAsync(
            ForId(ApiEndpoints.Organization.AccessRequests.Approve, Guid.NewGuid()),
            new ApproveAccessRequestRequest(Guid.NewGuid(), Guid.NewGuid(), "Anna", null, "Svensson", false),
            Token);
        using var reject = await host.Client.PostAsJsonAsync(
            ForId(ApiEndpoints.Organization.AccessRequests.Reject, Guid.NewGuid()),
            new RejectAccessRequestRequest("x"),
            Token);
        using var reopen = await host.Client.PostAsync(ForId(ApiEndpoints.Organization.AccessRequests.Reopen, Guid.NewGuid()), null, Token);

        // Assert
        list.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        approve.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        reject.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        reopen.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ALocalAccountHasNoEntraIdentityToRequestAccessWith()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);

        // Act
        using var request = await host.Client.PostAsync(Url(ApiEndpoints.Organization.AccessRequests.Me), null, Token);
        using var status = await host.Client.GetAsync(Url(ApiEndpoints.Organization.AccessRequests.Me), Token);

        // Assert
        request.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        status.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdministratorSeesAnEmptyListWhenNobodyHasApplied()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);

        // Act
        var items = await host.Client.GetFromJsonAsync<AccessRequestResponse[]>(Url(ApiEndpoints.Organization.AccessRequests.GetAll + "?status=Pending"), Token);

        // Assert
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task DecidingAnUnknownRequestReturnsNotFound()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);
        var missing = Guid.NewGuid();

        // Act
        using var approve = await host.Client.PostAsJsonAsync(
            ForId(ApiEndpoints.Organization.AccessRequests.Approve, missing),
            new ApproveAccessRequestRequest(Guid.NewGuid(), Guid.NewGuid(), "Anna", null, "Svensson", false),
            Token);
        using var reject = await host.Client.PostAsJsonAsync(
            ForId(ApiEndpoints.Organization.AccessRequests.Reject, missing),
            new RejectAccessRequestRequest("no"),
            Token);
        using var reopen = await host.Client.PostAsync(ForId(ApiEndpoints.Organization.AccessRequests.Reopen, missing), null, Token);

        // Assert
        approve.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        reject.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        reopen.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApprovingWithoutARoleOrCompanyIsAValidationError()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);

        // Act
        using var response = await host.Client.PostAsJsonAsync(
            ForId(ApiEndpoints.Organization.AccessRequests.Approve, Guid.NewGuid()),
            new ApproveAccessRequestRequest(Guid.Empty, Guid.Empty, "Anna", null, "Svensson", false),
            Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync(Token);
        body.ShouldContain(nameof(ApproveAccessRequestRequest.OwnCompanyId));
        body.ShouldContain(nameof(ApproveAccessRequestRequest.RoleId));
    }

    [Fact]
    public async Task ApprovingWithoutNamesOrWithABadNumberIsAValidationError()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(Token);

        // Act
        using var response = await host.Client.PostAsJsonAsync(
            ForId(ApiEndpoints.Organization.AccessRequests.Approve, Guid.NewGuid()),
            new ApproveAccessRequestRequest(Guid.NewGuid(), Guid.NewGuid(), " ", null, " ", false, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(3), "123"),
            Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync(Token);
        body.ShouldContain(nameof(ApproveAccessRequestRequest.FirstName));
        body.ShouldContain(nameof(ApproveAccessRequestRequest.LastName));
        body.ShouldContain(nameof(ApproveAccessRequestRequest.DateOfBirth));
        body.ShouldContain(nameof(ApproveAccessRequestRequest.SocialSecurityNumber));
    }
}
