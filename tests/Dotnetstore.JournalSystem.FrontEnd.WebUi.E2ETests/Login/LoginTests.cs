using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;
using Microsoft.Playwright;
using Shouldly;
using static Microsoft.Playwright.Assertions;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Login;

public sealed class LoginTests(E2EFixture fixture)
{
    [Fact]
    public async Task AnonymousVisitorAtTheRootLandsOnTheLoginPage()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);

        // Act
        await page.GotoAsync("/");

        // Assert
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Account/Login"));
        await Expect(login.Heading).ToBeVisibleAsync();
        await Expect(login.UsernameInput).ToBeVisibleAsync();
        await Expect(login.PasswordInput).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ValidCredentialsLandOnTheDashboard()
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
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Dashboard$"));
        await Expect(dashboard.Heading).ToBeVisibleAsync();
        await Expect(dashboard.CurrentUser).ToHaveTextAsync(E2EFixture.AdminFullName);
    }

    [Fact]
    public async Task WrongPasswordShowsAnErrorAndStaysOnTheLoginPage()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();

        // Act
        await login.SignInAsync(E2EFixture.AdminUsername, "not-the-right-password");

        // Assert
        await Expect(login.Error).ToHaveTextAsync("Invalid username or password.");
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Account/Login"));
        await Expect(login.PasswordInput).ToHaveValueAsync(string.Empty);
    }

    [Fact]
    public async Task UnknownUserGetsTheSameErrorAsAWrongPassword()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();

        // Act
        await login.SignInAsync("nobody@example.com", "whatever-password-123");

        // Assert
        await Expect(login.Error).ToHaveTextAsync("Invalid username or password.");
    }

    [Fact]
    public async Task EmptyFormShowsFieldErrorsWithoutCallingTheApi()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();

        // Act
        await login.SubmitButton.ClickAsync();

        // Assert
        await Expect(page.GetByText("Enter your username.")).ToBeVisibleAsync();
        await Expect(page.GetByText("Enter your password.")).ToBeVisibleAsync();
        await Expect(login.Error).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ProtectedPageRedirectsToLoginAndReturnsAfterSignIn()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        var dashboard = new DashboardPage(page);

        // Act
        await dashboard.GotoAsync();
        await Expect(login.Heading).ToBeVisibleAsync();
        await login.SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);

        // Assert
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Dashboard$"));
        await Expect(dashboard.Heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ExternalReturnUrlIsIgnoredAfterSignIn()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync("/Account/Login?ReturnUrl=https%3A%2F%2Fevil.example%2F");

        // Act
        await login.SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);

        // Assert
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Dashboard$"));
        new Uri(page.Url).Host.ShouldBe(new Uri(fixture.WebUiAddress).Host);
    }

    [Fact]
    public async Task SignedInVisitorOnTheLoginPageIsSentToTheDashboard()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();
        await login.SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Dashboard$"));

        // Act
        await login.GotoAsync();

        // Assert
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Dashboard$"));
    }

    [Fact]
    public async Task SigningOutReturnsToLoginAndProtectsTheDashboardAgain()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        var dashboard = new DashboardPage(page);
        await login.GotoAsync();
        await login.SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await Expect(dashboard.Heading).ToBeVisibleAsync();

        // Act
        await dashboard.LogoutButton.ClickAsync();

        // Assert
        await Expect(login.Heading).ToBeVisibleAsync();
        await dashboard.GotoAsync();
        await Expect(login.Heading).ToBeVisibleAsync();
    }
}
