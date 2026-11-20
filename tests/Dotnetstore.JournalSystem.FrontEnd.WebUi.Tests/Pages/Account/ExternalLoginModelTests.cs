using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Pages.Account;

public sealed class ExternalLoginModelTests
{
    private static EntraSignInSettings Enabled() =>
        EntraSignInSettings.FromConfiguration(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [EntraSignInSettings.EnabledKey] = "true",
                [EntraSignInSettings.TenantIdKey] = "tenant-1",
                [EntraSignInSettings.ClientIdKey] = "client-1",
                [EntraSignInSettings.ClientSecretKey] = "secret-1"
            })
            .Build());

    private static ExternalLoginModel Create(EntraSignInSettings settings)
    {
        var model = new ExternalLoginModel(settings);
        model.PageContext = new PageContext();
        model.Url = new StubUrlHelper();
        return model;
    }

    private sealed class StubUrlHelper : IUrlHelper
    {
        public Microsoft.AspNetCore.Mvc.ActionContext ActionContext { get; } = new();

        public string? Action(Microsoft.AspNetCore.Mvc.Routing.UrlActionContext actionContext) => null;

        public string? Content(string? contentPath) => contentPath;

        public bool IsLocalUrl(string? url) => url is ['/', ..] && url is not ['/', '/', ..];

        public string? Link(string? routeName, object? values) => null;

        public string? RouteUrl(Microsoft.AspNetCore.Mvc.Routing.UrlRouteContext routeContext) => null;
    }

    [Fact]
    public void ChallengesTheEntraSchemeAndReturnsToTheDashboardByDefault()
    {
        // Arrange
        var model = Create(Enabled());

        // Act
        var result = model.OnGet(null);

        // Assert
        var challenge = result.ShouldBeOfType<ChallengeResult>();
        challenge.AuthenticationSchemes.ShouldBe([EntraSignInSettings.SchemeName]);
        challenge.Properties!.RedirectUri.ShouldBe("/Dashboard");
    }

    [Fact]
    public void KeepsALocalReturnUrl()
    {
        // Arrange
        var model = Create(Enabled());

        // Act
        var result = model.OnGet("/Dashboard?x=1");

        // Assert
        result.ShouldBeOfType<ChallengeResult>().Properties!.RedirectUri.ShouldBe("/Dashboard?x=1");
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("//evil.example/")]
    public void IgnoresAReturnUrlThatLeavesTheSite(string returnUrl)
    {
        // Arrange
        var model = Create(Enabled());

        // Act
        var result = model.OnGet(returnUrl);

        // Assert
        result.ShouldBeOfType<ChallengeResult>().Properties!.RedirectUri.ShouldBe("/Dashboard");
    }

    [Fact]
    public void IsNotFoundWhenMicrosoftSignInIsNotEnabled()
    {
        // Arrange
        var model = Create(EntraSignInSettings.Disabled);

        // Act
        var result = model.OnGet("/Dashboard");

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }
}
