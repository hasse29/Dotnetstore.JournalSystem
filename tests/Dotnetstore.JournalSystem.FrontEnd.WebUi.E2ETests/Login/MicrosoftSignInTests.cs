using System.Text.RegularExpressions;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;
using Shouldly;
using static Microsoft.Playwright.Assertions;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Login;

// These tests share the fake identity provider's one-shot error switch, so they live in one class and run in order.
[Collection(MicrosoftSignInSerial.Name)]
public sealed class MicrosoftSignInTests(E2EFixture fixture)
{
    [Fact]
    public async Task LoginPageOffersMicrosoftSignInNextToThePasswordForm()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);

        // Act
        await login.GotoAsync();

        // Assert
        await Expect(login.MicrosoftButton).ToBeVisibleAsync();
        await Expect(login.UsernameInput).ToBeVisibleAsync();
    }

    [Fact]
    public async Task MicrosoftSignInLandsOnTheDashboardAsTheEntraUser()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        var dashboard = new DashboardPage(page);
        await login.GotoAsync();

        // Act
        await login.MicrosoftButton.ClickAsync();

        // Assert
        await Expect(page).ToHaveURLAsync(new Regex("/Dashboard$"));
        await Expect(dashboard.Heading).ToBeVisibleAsync();
        await Expect(dashboard.CurrentUser).ToHaveTextAsync(E2EFixture.AdminFullName);
        await Expect(dashboard.StatValue("roles")).ToHaveTextAsync(new Regex("^[0-9]+$"));
    }

    [Fact]
    public async Task ProtectedPageSignedInThroughMicrosoftReturnsToThePageThatWasAskedFor()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();
        await Expect(login.Heading).ToBeVisibleAsync();

        // Act
        await login.MicrosoftButton.ClickAsync();

        // Assert
        await Expect(page).ToHaveURLAsync(new Regex("/Dashboard$"));
        await Expect(dashboard.Heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ExternalReturnUrlIsIgnoredAfterMicrosoftSignIn()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();

        // Act
        await page.GotoAsync("/Account/ExternalLogin?returnUrl=https%3A%2F%2Fevil.example%2F");

        // Assert
        await Expect(page).ToHaveURLAsync(new Regex("/Dashboard$"));
        new Uri(page.Url).Host.ShouldBe(new Uri(fixture.WebUiAddress).Host);
    }

    [Fact]
    public async Task CancelledMicrosoftSignInReturnsToLoginWithAMessage()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();
        fixture.Idp.NextAuthorizeError = "access_denied";

        // Act
        await login.MicrosoftButton.ClickAsync();

        // Assert
        await Expect(login.Heading).ToBeVisibleAsync();
        await Expect(login.Error).ToContainTextAsync("Microsoft sign-in did not complete (access_denied)");
        var dashboard = new DashboardPage(page);
        await dashboard.GotoAsync();
        await Expect(login.Heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task MicrosoftSignInTheApiRejectsReturnsToLoginWithAMessage()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();
        fixture.Idp.NextAccessToken = "not-a-token-the-api-accepts";

        // Act
        await login.MicrosoftButton.ClickAsync();

        // Assert
        await Expect(login.Heading).ToBeVisibleAsync();
        await Expect(login.Error).ToContainTextAsync("the API did not accept your sign-in");
    }

    [Fact]
    public async Task SigningOutAfterMicrosoftSignInProtectsTheDashboardAgain()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        var dashboard = new DashboardPage(page);
        await login.GotoAsync();
        await login.MicrosoftButton.ClickAsync();
        await Expect(dashboard.Heading).ToBeVisibleAsync();

        // Act
        await dashboard.LogoutButton.ClickAsync();

        // Assert
        await Expect(login.Heading).ToBeVisibleAsync();
        await dashboard.GotoAsync();
        await Expect(login.Heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task BackButtonAfterSigningOutDoesNotShowTheDashboardAgain()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        var dashboard = new DashboardPage(page);
        await login.GotoAsync();
        await login.MicrosoftButton.ClickAsync();
        await Expect(dashboard.Heading).ToBeVisibleAsync();
        var reloaded = await page.ReloadAsync();
        reloaded!.Headers["cache-control"].ShouldContain("no-store");
        await dashboard.LogoutButton.ClickAsync();
        await Expect(login.Heading).ToBeVisibleAsync();

        // Act
        await page.GoBackAsync();

        // Assert
        await Expect(login.Heading).ToBeVisibleAsync();
        await Expect(dashboard.Heading).ToBeHiddenAsync();
    }

    [Fact]
    public async Task PasswordSignInStillWorksWhenMicrosoftSignInIsAvailable()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        var dashboard = new DashboardPage(page);
        await login.GotoAsync();

        // Act
        await login.SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);

        // Assert
        await Expect(dashboard.CurrentUser).ToHaveTextAsync(E2EFixture.AdminFullName);
    }
}
