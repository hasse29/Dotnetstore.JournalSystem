using System.Text.RegularExpressions;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Dashboard;

public sealed class DashboardTests(E2EFixture fixture)
{
    private async Task<(IBrowserContext Context, IPage Page, DashboardPage Dashboard)> SignInAsync(
        string username,
        string password,
        ViewportSize? viewport = null)
    {
        var context = await fixture.NewContextAsync(viewport);
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();
        await login.SignInAsync(username, password);
        var dashboard = new DashboardPage(page);
        await Expect(dashboard.Heading).ToBeVisibleAsync();
        return (context, page, dashboard);
    }

    [Fact]
    public async Task AdministratorSeesSummaryFiguresAndTheirOwnSignIn()
    {
        // Arrange
        var (context, _, dashboard) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;

        // Act
        await dashboard.GotoAsync();

        // Assert
        await Expect(dashboard.WelcomeName).ToHaveTextAsync(E2EFixture.AdminUsername);
        await Expect(dashboard.StatValue("users")).ToHaveTextAsync(new Regex("^[1-9][0-9]*$"));
        await Expect(dashboard.StatValue("roles")).ToHaveTextAsync(new Regex("^[1-9][0-9]*$"));
        await Expect(dashboard.StatValue("companies")).ToHaveTextAsync(new Regex("^[1-9][0-9]*$"));
        await Expect(dashboard.RecentTable).ToContainTextAsync(E2EFixture.AdminUsername);
    }

    [Fact]
    public async Task SidebarMarksTheDashboardAsTheActivePage()
    {
        // Arrange
        var (context, _, dashboard) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;

        // Assert
        await Expect(dashboard.NavDashboard).ToHaveClassAsync(new Regex("active"));
        await Expect(dashboard.Sidebar).ToBeVisibleAsync();
    }

    [Fact]
    public async Task UserWithoutAdministratorRoleStillGetsADashboardButNoUserFigures()
    {
        // Arrange
        var username = await fixture.CreateUserAsync("Nurse");
        var (context, _, dashboard) = await SignInAsync(username, E2EFixture.UserPassword);
        await using var _ = context;

        // Assert
        await Expect(dashboard.CurrentUser).ToHaveTextAsync(E2EFixture.UserFullName);
        await Expect(dashboard.StatValue("users")).ToHaveCountAsync(0);
        await Expect(dashboard.RecentTable).ToHaveCountAsync(0);
        await Expect(dashboard.StatValue("roles")).ToHaveTextAsync(new Regex("^[0-9]+$"));
    }

    [Fact]
    public async Task OnAPhoneTheSidebarIsHiddenUntilTheMenuButtonIsPressed()
    {
        // Arrange
        var (context, _, dashboard) = await SignInAsync(
            E2EFixture.AdminUsername,
            E2EFixture.AdminPassword,
            new ViewportSize { Width = 390, Height = 800 });
        await using var _ = context;

        // Act & Assert
        await Expect(dashboard.SidebarToggle).ToBeVisibleAsync();
        await Expect(dashboard.NavDashboard).Not.ToBeInViewportAsync();

        await dashboard.SidebarToggle.ClickAsync();
        await Expect(dashboard.NavDashboard).ToBeInViewportAsync();

        await dashboard.SidebarToggle.ClickAsync();
        await Expect(dashboard.NavDashboard).Not.ToBeInViewportAsync();
    }

    [Fact]
    public async Task OnADesktopTheMenuButtonIsHidden()
    {
        // Arrange
        var (context, _, dashboard) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;

        // Assert
        await Expect(dashboard.SidebarToggle).Not.ToBeVisibleAsync();
    }
}
