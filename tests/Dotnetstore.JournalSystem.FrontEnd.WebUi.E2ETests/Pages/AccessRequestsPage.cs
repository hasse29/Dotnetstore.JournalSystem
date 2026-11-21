using Microsoft.Playwright;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;

internal sealed class AccessRequestsPage(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Access requests", Level = 1 });

    public ILocator Message => page.GetByTestId("access-message");

    public ILocator Error => page.GetByTestId("access-error");

    public ILocator Forbidden => page.GetByTestId("access-forbidden");

    public ILocator NavLink => page.GetByTestId("nav-access-requests");

    public ILocator Pending(string username) => page.GetByTestId($"request-{username}");

    public ILocator Decided(string username) => page.GetByTestId($"decided-{username}");

    public async Task GotoAsync() => await page.GotoAsync("/Admin/AccessRequests");

    public async Task ApproveAsync(string username, string roleName)
    {
        var row = Pending(username);
        await row.GetByTestId("role-select").SelectOptionAsync(new SelectOptionValue { Label = roleName });
        await row.GetByTestId("approve").ClickAsync();
    }

    public async Task RejectAsync(string username, string note)
    {
        var row = Pending(username);
        await row.GetByTestId("reject-note").FillAsync(note);
        await row.GetByTestId("reject").ClickAsync();
    }

    public async Task ReopenAsync(string username) => await Decided(username).GetByTestId("reopen").ClickAsync();
}
