using Microsoft.Playwright;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;

internal sealed class RolesPage(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Roles", Level = 1 });

    public ILocator NavLink => page.GetByTestId("nav-roles");

    public ILocator Table => page.GetByTestId("roles-table");

    public ILocator Message => page.GetByTestId("roles-message");

    public ILocator Error => page.GetByTestId("roles-error");

    public ILocator AddButton => page.GetByTestId("role-add");

    public ILocator CreateForm => page.GetByTestId("role-create-form");

    public ILocator Row(string name) => page.GetByTestId($"role-{name}");

    public async Task GotoAsync() => await page.GotoAsync("/Roles");

    public async Task AddAsync(string name, string description)
    {
        await AddButton.ClickAsync();
        await CreateForm.GetByTestId("role-name").FillAsync(name);
        await CreateForm.GetByTestId("role-description").FillAsync(description);
        await CreateForm.GetByTestId("role-create").ClickAsync();
    }

    public async Task RenameAsync(string name, string newName)
    {
        var row = Row(name);
        await row.GetByTestId("role-edit").ClickAsync();
        await row.GetByTestId("role-name").FillAsync(newName);
        await row.GetByTestId("role-save").ClickAsync();
    }

    public async Task DeleteAsync(string name)
    {
        page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
        await Row(name).GetByTestId("role-delete").ClickAsync();
    }
}
