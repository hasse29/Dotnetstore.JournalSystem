using Microsoft.Playwright;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;

internal sealed class DashboardPage(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Dashboard", Level = 1 });

    public ILocator CurrentUser => page.GetByTestId("current-user");

    public ILocator CurrentUserDetail => page.GetByTestId("current-user-detail");

    public ILocator WelcomeName => page.GetByTestId("welcome-name");

    public ILocator LogoutButton => page.GetByTestId("logout");

    public ILocator SidebarToggle => page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Toggle navigation" });

    public ILocator Sidebar => page.GetByRole(AriaRole.Complementary, new PageGetByRoleOptions { Name = "Main navigation" });

    public ILocator NavDashboard => page.GetByTestId("nav-dashboard");

    public ILocator StatValue(string key) => page.GetByTestId($"stat-{key}-value");

    public ILocator RecentTable => page.GetByTestId("recent-table");

    public async Task GotoAsync() => await page.GotoAsync("/Dashboard");
}
