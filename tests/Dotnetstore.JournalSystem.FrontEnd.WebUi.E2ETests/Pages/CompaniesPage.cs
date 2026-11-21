using Microsoft.Playwright;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;

internal sealed class CompaniesPage(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Own companies", Level = 1 });

    public ILocator NavLink => page.GetByTestId("nav-companies");

    public ILocator Count => page.GetByTestId("companies-count");

    public ILocator Table => page.GetByTestId("companies-table");

    public ILocator Row(string name) => page.GetByTestId($"company-{name}");

    public async Task GotoAsync() => await page.GotoAsync("/Companies");
}
