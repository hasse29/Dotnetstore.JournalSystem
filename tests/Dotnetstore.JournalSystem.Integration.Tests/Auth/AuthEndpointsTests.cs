using System.Net;
using System.Net.Http.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Auth;

public sealed class AuthEndpointsTests(SqlServerFixture fixture)
{
    private static Uri LoginUrl => new(ApiEndpoints.Auth.Login, UriKind.Relative);

    [Fact]
    public async Task LoginWithCorrectCredentialsReturnsAdministratorToken()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken, withBootstrapAdmin: true);
        var request = new LoginRequest(ApiTestHost.AdminUsername, ApiTestHost.AdminPassword);

        // Act
        using var response = await host.Client.PostAsJsonAsync(LoginUrl, request, TestContext.Current.CancellationToken);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        login.ShouldNotBeNull().AccessToken.ShouldNotBeNullOrWhiteSpace();
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(login.AccessToken);
        jwt.Claims.ShouldContain(x => x.Type == AuthClaimTypes.Role && x.Value == RoleNames.Administrator);
    }

    [Fact]
    public async Task LoginWithWrongPasswordReturnsUnauthorized()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken, withBootstrapAdmin: true);
        var request = new LoginRequest(ApiTestHost.AdminUsername, "not-the-right-password");

        // Act
        using var response = await host.Client.PostAsJsonAsync(LoginUrl, request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginWithUnknownUserReturnsUnauthorized()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken, withBootstrapAdmin: true);
        var request = new LoginRequest("nobody@example.com", "whatever-password-123");

        // Act
        using var response = await host.Client.PostAsJsonAsync(LoginUrl, request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginWithBlankFieldsReturnsValidationProblem()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken, withBootstrapAdmin: true);
        var request = new LoginRequest(" ", string.Empty);

        // Act
        using var response = await host.Client.PostAsJsonAsync(LoginUrl, request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RepeatedFailuresLockTheAccountEvenForTheCorrectPassword()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken, withBootstrapAdmin: true);
        var wrong = new LoginRequest(ApiTestHost.AdminUsername, "not-the-right-password");
        for (var i = 0; i < 5; i++)
        {
            using var failed = await host.Client.PostAsJsonAsync(LoginUrl, wrong, TestContext.Current.CancellationToken);
            failed.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        // Act
        var correct = new LoginRequest(ApiTestHost.AdminUsername, ApiTestHost.AdminPassword);
        using var response = await host.Client.PostAsJsonAsync(LoginUrl, correct, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginIsRateLimitedPerClient()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var request = new LoginRequest("nobody@example.com", "whatever-password-123");
        HttpStatusCode last = default;

        // Act
        for (var i = 0; i < 12; i++)
        {
            using var response = await host.Client.PostAsJsonAsync(LoginUrl, request, TestContext.Current.CancellationToken);
            last = response.StatusCode;
        }

        // Assert
        last.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
