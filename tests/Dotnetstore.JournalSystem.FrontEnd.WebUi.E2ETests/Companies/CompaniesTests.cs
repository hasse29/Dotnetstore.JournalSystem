using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Companies;

public sealed class CompaniesTests(E2EFixture fixture)
{
    private static string UniqueName(string prefix) => $"{prefix} {Guid.NewGuid():N}"[..30];

    private async Task<(IBrowserContext Context, CompaniesPage Companies)> SignInAsync(string username, string password)
    {
        var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();
        await login.SignInAsync(username, password);
        await Expect(new DashboardPage(page).Heading).ToBeVisibleAsync();
        var companies = new CompaniesPage(page);
        await companies.NavLink.ClickAsync();
        await Expect(companies.Heading).ToBeVisibleAsync();
        return (context, companies);
    }

    [Fact]
    public async Task AdministratorCanAddRenameAndDeleteACompany()
    {
        // Arrange
        var (context, companies) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;
        var name = UniqueName("Acme");
        var renamed = UniqueName("Renamed");

        // Act
        await companies.AddAsync(name, "Added from the browser");
        await Expect(companies.Message).ToHaveTextAsync("Company created.");
        await Expect(companies.Row(name)).ToContainTextAsync("Added from the browser");

        await companies.RenameAsync(name, renamed);
        await Expect(companies.Message).ToHaveTextAsync("Company updated.");
        await Expect(companies.Row(renamed)).ToBeVisibleAsync();
        await Expect(companies.Row(name)).ToHaveCountAsync(0);

        await companies.DeleteAsync(renamed);

        // Assert
        await Expect(companies.Message).ToHaveTextAsync("Company deleted.");
        await Expect(companies.Row(renamed)).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task TheSystemCompanyIsMarkedAndHasNoDeleteButton()
    {
        // Arrange
        var (context, companies) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;

        // Act
        var system = companies.Row("Test AB");

        // Assert
        await Expect(system).ToBeVisibleAsync();
        await Expect(system.GetByTestId("company-system")).ToBeVisibleAsync();
        await Expect(system.GetByTestId("company-edit")).ToBeVisibleAsync();
        await Expect(system.GetByTestId("company-delete")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task AddingACompanyWithoutANameIsRefusedInTheBrowser()
    {
        // Arrange
        var (context, companies) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;

        // Act
        await companies.AddButton.ClickAsync();
        await companies.CreateForm.GetByTestId("company-create").ClickAsync();

        // Assert
        await Expect(companies.CreateDialog).ToBeVisibleAsync();
        await Expect(companies.Message).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task CancellingTheAddDialogClosesItWithoutSavingAnything()
    {
        // Arrange
        var (context, companies) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;
        var name = UniqueName("Cancelled");
        await companies.AddButton.ClickAsync();
        await companies.CreateForm.GetByTestId("company-name").FillAsync(name);

        // Act
        await companies.CreateForm.GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Cancel" }).ClickAsync();

        // Assert
        await Expect(companies.CreateDialog).ToBeHiddenAsync();
        await Expect(companies.Row(name)).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task NonAdministratorSeesTheListButCannotChangeIt()
    {
        // Arrange
        var username = await fixture.CreateUserAsync("Nurse");
        var (context, companies) = await SignInAsync(username, E2EFixture.UserPassword);
        await using var _ = context;

        // Act
        var row = companies.Row("Test AB");

        // Assert
        await Expect(companies.Table).ToBeVisibleAsync();
        await Expect(row).ToBeVisibleAsync();
        await Expect(companies.AddButton).ToHaveCountAsync(0);
        await Expect(companies.CreateForm).ToHaveCountAsync(0);
        await Expect(row.GetByTestId("company-edit")).ToHaveCountAsync(0);
        await Expect(row.GetByTestId("company-delete")).ToHaveCountAsync(0);
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
