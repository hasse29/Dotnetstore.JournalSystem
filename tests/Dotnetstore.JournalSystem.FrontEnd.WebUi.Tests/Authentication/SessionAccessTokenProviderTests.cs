using System.Security.Claims;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Authentication;

public sealed class SessionAccessTokenProviderTests
{
    private sealed class StubAuthenticationService(AuthenticateResult result) : IAuthenticationService
    {
        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) => Task.FromResult(result);

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
    }

    private static SessionAccessTokenProvider CreateProvider(AuthenticateResult? result)
    {
        var context = new DefaultHttpContext();
        if (result is not null)
        {
            context.RequestServices = new ServiceCollection()
                .AddSingleton<IAuthenticationService>(new StubAuthenticationService(result))
                .BuildServiceProvider();
        }

        return new SessionAccessTokenProvider(new HttpContextAccessor { HttpContext = context });
    }

    [Fact]
    public async Task ReturnsTheTokenStoredInTheSession()
    {
        // Arrange
        var properties = new AuthenticationProperties();
        properties.StoreTokens([new AuthenticationToken { Name = SessionAccessTokenProvider.AccessTokenName, Value = "abc" }]);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity("test")), properties, CookieAuthenticationDefaults.AuthenticationScheme);
        var provider = CreateProvider(AuthenticateResult.Success(ticket));

        // Act
        var token = await provider.GetAccessTokenAsync(TestContext.Current.CancellationToken);

        // Assert
        token.ShouldBe("abc");
    }

    [Fact]
    public async Task ReturnsNullWhenTheVisitorIsNotSignedIn()
    {
        // Arrange
        var provider = CreateProvider(AuthenticateResult.NoResult());

        // Act
        var token = await provider.GetAccessTokenAsync(TestContext.Current.CancellationToken);

        // Assert
        token.ShouldBeNull();
    }

    [Fact]
    public async Task ReturnsNullOutsideOfAWebRequest()
    {
        // Arrange
        var provider = new SessionAccessTokenProvider(new HttpContextAccessor());

        // Act
        var token = await provider.GetAccessTokenAsync(TestContext.Current.CancellationToken);

        // Assert
        token.ShouldBeNull();
    }
}
