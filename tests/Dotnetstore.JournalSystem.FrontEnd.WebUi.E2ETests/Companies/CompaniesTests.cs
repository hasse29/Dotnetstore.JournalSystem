using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;
using static Microsoft.Playwright.Assertions;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Companies;

public sealed class CompaniesTests(E2EFixture fixture)
{
    [Fact]
    public async Task SignedInUserSeesTheListOfOwnCompaniesFromTheNavigation()
    {
        // Arrange
        var username = await fixture.CreateUserAsync("Nurse");
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        var dashboard = new DashboardPage(page);
        var companies = new CompaniesPage(page);
        await login.GotoAsync();
        await login.SignInAsync(username, E2EFixture.UserPassword);
        await Expect(dashboard.Heading).ToBeVisibleAsync();

        // Act
        await companies.NavLink.ClickAsync();

        // Assert
        await Expect(companies.Heading).ToBeVisibleAsync();
        await Expect(companies.Table).ToBeVisibleAsync();
        await Expect(companies.Row("Test AB")).ToBeVisibleAsync();
        await Expect(companies.Count).ToHaveTextAsync("1");
    }

    [Fact]
    public async Task CompaniesPageRequiresSignIn()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();

        // Act
        await new CompaniesPage(page).GotoAsync();

        // Assert
        await Expect(new LoginPage(page).Heading).ToBeVisibleAsync();
    }
}
