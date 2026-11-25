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

    public ILocator FirstNameInput(string username) => Pending(username).GetByTestId("first-name");

    public ILocator LastNameInput(string username) => Pending(username).GetByTestId("last-name");

    public ILocator Decided(string username) => page.GetByTestId($"decided-{username}");

    public async Task GotoAsync() => await page.GotoAsync("/Admin/AccessRequests");

    public async Task ApproveAsync(
        string username,
        string roleName,
        string gender = "female",
        string? dateOfBirth = null,
        string? personalId = null)
    {
        var row = Pending(username);
        await row.GetByTestId("gender").SelectOptionAsync(new SelectOptionValue { Value = gender });
        if (dateOfBirth is not null)
        {
            await row.GetByTestId("date-of-birth").FillAsync(dateOfBirth);
        }

        if (personalId is not null)
        {
            await row.GetByTestId("personal-id").FillAsync(personalId);
        }

        await row.GetByTestId("role-select").SelectOptionAsync(new SelectOptionValue { Label = roleName });
        // Other tests add companies, so the form cannot be relied on to preselect the only one.
        await row.GetByTestId("company-select").SelectOptionAsync(new SelectOptionValue { Label = "Test AB" });
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
