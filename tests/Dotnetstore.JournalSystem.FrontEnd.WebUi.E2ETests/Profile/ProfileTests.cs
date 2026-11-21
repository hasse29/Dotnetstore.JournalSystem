using System.Text.RegularExpressions;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;
using static Microsoft.Playwright.Assertions;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Profile;

public sealed class ProfileTests(E2EFixture fixture)
{
    [Fact]
    public async Task PasswordUserSeesTheirNameRoleAndCompanyInTheTopBarAndOnTheProfilePage()
    {
        // Arrange
        var username = await fixture.CreateUserAsync("Nurse");
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        var dashboard = new DashboardPage(page);
        var profile = new ProfilePage(page);
        await login.GotoAsync();
        await login.SignInAsync(username, E2EFixture.UserPassword);
        await Expect(dashboard.Heading).ToBeVisibleAsync();

        // Act
        await profile.NavLink.ClickAsync();

        // Assert
        await Expect(dashboard.CurrentUser).ToHaveTextAsync(E2EFixture.UserFullName);
        await Expect(dashboard.CurrentUserDetail).ToContainTextAsync("Test AB");
        await Expect(profile.Heading).ToBeVisibleAsync();
        await Expect(profile.Name).ToHaveTextAsync(E2EFixture.UserFullName);
        await Expect(profile.Username).ToHaveTextAsync(username);
        await Expect(profile.Provider).ToHaveTextAsync("Password");
        await Expect(profile.Company).ToHaveTextAsync("Test AB");
        await Expect(profile.Role).ToHaveTextAsync(new Regex("^Nurse-"));
        await Expect(profile.PersonalId).ToHaveTextAsync("–");
    }

    [Fact]
    public async Task ProfilePageRequiresSignIn()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();

        // Act
        await new ProfilePage(page).GotoAsync();

        // Assert
        await Expect(new LoginPage(page).Heading).ToBeVisibleAsync();
    }
}
