using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Roles;

public sealed class RolesTests(E2EFixture fixture)
{
    private static string UniqueName(string prefix) => $"{prefix} {Guid.NewGuid():N}"[..30];

    private async Task<(IBrowserContext Context, RolesPage Roles)> SignInAsync(string username, string password)
    {
        var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();
        await login.SignInAsync(username, password);
        await Expect(new DashboardPage(page).Heading).ToBeVisibleAsync();
        var roles = new RolesPage(page);
        await roles.NavLink.ClickAsync();
        await Expect(roles.Heading).ToBeVisibleAsync();
        return (context, roles);
    }

    [Fact]
    public async Task AdministratorCanAddRenameAndDeleteARole()
    {
        // Arrange
        var (context, roles) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;
        var name = UniqueName("Doctor");
        var renamed = UniqueName("Surgeon");

        // Act
        await roles.AddAsync(name, "Added from the browser");
        await Expect(roles.Message).ToHaveTextAsync("Role created.");
        await Expect(roles.Row(name)).ToContainTextAsync("Added from the browser");

        await roles.RenameAsync(name, renamed);
        await Expect(roles.Message).ToHaveTextAsync("Role updated.");
        await Expect(roles.Row(renamed)).ToBeVisibleAsync();
        await Expect(roles.Row(name)).ToHaveCountAsync(0);

        await roles.DeleteAsync(renamed);

        // Assert
        await Expect(roles.Message).ToHaveTextAsync("Role deleted.");
        await Expect(roles.Row(renamed)).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ATakenNameIsRefusedWithTheReason()
    {
        // Arrange
        var (context, roles) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;

        // Act
        await roles.AddAsync("Administrator", "A second one");

        // Assert
        await Expect(roles.Error).ToHaveTextAsync("A role with that name already exists.");
        await Expect(roles.Row("Administrator")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task TheAdministratorRoleIsMarkedAndCannotBeRenamedOrDeleted()
    {
        // Arrange
        var (context, roles) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;
        var system = roles.Row("Administrator");

        // Act
        await system.GetByTestId("role-edit").ClickAsync();

        // Assert
        await Expect(system.GetByTestId("role-system")).ToBeVisibleAsync();
        await Expect(system.GetByTestId("role-name")).Not.ToBeEditableAsync();
        await Expect(system.GetByTestId("role-description")).ToBeEditableAsync();
        await Expect(system.GetByTestId("role-delete")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ARoleThatAUserHasCannotBeDeleted()
    {
        // Arrange
        await fixture.CreateUserAsync("Porter");
        var (context, roles) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;
        var row = roles.Table.GetByRole(AriaRole.Row).Filter(new LocatorFilterOptions { HasText = "Porter-" }).First;
        var name = (await row.Locator("strong").InnerTextAsync()).Trim();

        // Act
        await roles.DeleteAsync(name);

        // Assert
        await Expect(roles.Error).ToHaveTextAsync("The role is still used by users. Give them another role first.");
        await Expect(roles.Row(name)).ToBeVisibleAsync();
    }

    [Fact]
    public async Task NonAdministratorSeesTheListButCannotChangeIt()
    {
        // Arrange
        var username = await fixture.CreateUserAsync("Nurse");
        var (context, roles) = await SignInAsync(username, E2EFixture.UserPassword);
        await using var _ = context;

        // Act
        var row = roles.Row("Administrator");

        // Assert
        await Expect(row).ToBeVisibleAsync();
        await Expect(roles.AddButton).ToHaveCountAsync(0);
        await Expect(roles.CreateForm).ToHaveCountAsync(0);
        await Expect(row.GetByTestId("role-edit")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task RolesPageRequiresSignIn()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();

        // Act
        await new RolesPage(page).GotoAsync();

        // Assert
        await Expect(new LoginPage(page).Heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task TickingARightTicksWhatItRequiresAndTheRightsAreSaved()
    {
        // Arrange
        var (context, roles) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;
        var name = UniqueName("Manager");
        await roles.AddButton.ClickAsync();
        await roles.CreateForm.GetByTestId("role-name").FillAsync(name);

        // Act
        await roles.CreateForm.GetByTestId($"permission-{Permissions.Users.Edit}").CheckAsync();
        await Expect(roles.CreateForm.GetByTestId($"permission-{Permissions.Users.View}")).ToBeCheckedAsync();
        await Expect(roles.CreateForm.GetByTestId($"permission-{Permissions.OwnCompanies.View}")).ToBeCheckedAsync();
        await roles.CreateForm.GetByTestId("role-create").ClickAsync();
        await Expect(roles.Message).ToHaveTextAsync("Role created.");

        // Assert
        var row = roles.Row(name);
        await row.GetByTestId("role-edit").ClickAsync();
        await Expect(row.GetByTestId($"permission-{Permissions.Users.Edit}")).ToBeCheckedAsync();
        await Expect(row.GetByTestId($"permission-{Permissions.Roles.View}")).ToBeCheckedAsync();
        await Expect(row.GetByTestId($"permission-{Permissions.Users.Delete}")).Not.ToBeCheckedAsync();

        await row.GetByTestId($"permission-{Permissions.Roles.View}").UncheckAsync();
        await Expect(row.GetByTestId($"permission-{Permissions.Users.View}")).Not.ToBeCheckedAsync();
        await Expect(row.GetByTestId($"permission-{Permissions.Users.Edit}")).Not.ToBeCheckedAsync();
    }

    [Fact]
    public async Task SomeoneWithOnlyTheRightToAddCompaniesIsOfferedJustThat()
    {
        // Arrange
        var username = await fixture.CreateUserAsync(
            "Registrar",
            permissions: [Permissions.OwnCompanies.View, Permissions.OwnCompanies.Create]);
        var context = await fixture.NewContextAsync();
        await using var _ = context;
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();
        await login.SignInAsync(username, E2EFixture.UserPassword);
        await Expect(new DashboardPage(page).Heading).ToBeVisibleAsync();
        var companies = new CompaniesPage(page);

        // Act
        await companies.NavLink.ClickAsync();

        // Assert
        await Expect(companies.AddButton).ToBeVisibleAsync();
        await Expect(companies.Row("Test AB").GetByTestId("company-edit")).ToHaveCountAsync(0);
        await Expect(new RolesPage(page).NavLink).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task SomeoneWithNoRightsHasNoLinksAndIsToldWhy()
    {
        // Arrange
        var username = await fixture.CreateUserAsync("Visitor", permissions: []);
        var context = await fixture.NewContextAsync();
        await using var _ = context;
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();
        await login.SignInAsync(username, E2EFixture.UserPassword);
        await Expect(new DashboardPage(page).Heading).ToBeVisibleAsync();

        // Act
        await new RolesPage(page).GotoAsync();

        // Assert
        await Expect(page.GetByTestId("roles-forbidden")).ToBeVisibleAsync();
        await Expect(new RolesPage(page).NavLink).ToHaveCountAsync(0);
        await Expect(new CompaniesPage(page).NavLink).ToHaveCountAsync(0);
    }
}
