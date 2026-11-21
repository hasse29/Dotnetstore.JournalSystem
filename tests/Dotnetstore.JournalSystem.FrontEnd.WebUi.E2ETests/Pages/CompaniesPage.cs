using Microsoft.Playwright;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;

internal sealed class CompaniesPage(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Own companies", Level = 1 });

    public ILocator NavLink => page.GetByTestId("nav-companies");

    public ILocator Count => page.GetByTestId("companies-count");

    public ILocator Table => page.GetByTestId("companies-table");

    public ILocator Message => page.GetByTestId("companies-message");

    public ILocator Error => page.GetByTestId("companies-error");

    public ILocator AddButton => page.GetByTestId("company-add");

    public ILocator CreateForm => page.GetByTestId("company-create-form");

    public ILocator CreateDialog => page.Locator("#company-create-dialog");

    public ILocator Row(string name) => page.GetByTestId($"company-{name}");

    public async Task GotoAsync() => await page.GotoAsync("/Companies");

    public async Task AddAsync(string name, string description)
    {
        await AddButton.ClickAsync();
        await CreateForm.GetByTestId("company-name").FillAsync(name);
        await CreateForm.GetByTestId("company-description").FillAsync(description);
        await CreateForm.GetByTestId("company-create").ClickAsync();
    }

    public async Task RenameAsync(string name, string newName)
    {
        var row = Row(name);
        await row.GetByTestId("company-edit").ClickAsync();
        await row.GetByTestId("company-name").FillAsync(newName);
        await row.GetByTestId("company-save").ClickAsync();
    }

    public async Task DeleteAsync(string name)
    {
        page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
        await Row(name).GetByTestId("company-delete").ClickAsync();
    }
}
