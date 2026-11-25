using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;
using Microsoft.Playwright;
using Shouldly;
using static Microsoft.Playwright.Assertions;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Companies;

public sealed class CompanyDetailsTests(E2EFixture fixture)
{
    private static string UniqueName(string prefix) => $"{prefix} {Guid.NewGuid():N}"[..30];

    private async Task<(IBrowserContext Context, IPage Page)> SignInAsync(string username, string password)
    {
        var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        await login.GotoAsync();
        await login.SignInAsync(username, password);
        await Expect(new DashboardPage(page).Heading).ToBeVisibleAsync();
        await new CompaniesPage(page).NavLink.ClickAsync();
        return (context, page);
    }

    private static async Task<CompanyDetailsPage> OpenAsync(IPage page, string name)
    {
        await new CompaniesPage(page).Row(name).GetByTestId("company-open").ClickAsync();
        var details = new CompanyDetailsPage(page);
        await Expect(details.Heading).ToHaveTextAsync(name);
        return details;
    }

    private async Task<(IBrowserContext Context, CompanyDetailsPage Details, string Name)> OpenNewCompanyAsync()
    {
        var (context, page) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        var name = UniqueName("Detail");
        await new CompaniesPage(page).AddAsync(name, "A company with a page of its own");
        var details = await OpenAsync(page, name);
        details.AcceptConfirmations();
        return (context, details, name);
    }

    [Fact]
    public async Task AdministratorCanEditTheCompanyAndSeeWhoChangedIt()
    {
        // Arrange
        var (context, details, _) = await OpenNewCompanyAsync();
        await using var __ = context;

        // Act
        await details.SetCorporateNumberAsync("556677-8899");

        // Assert
        await Expect(details.Message).ToHaveTextAsync("Company updated.");
        await Expect(details.CorporateNumber).ToHaveTextAsync("556677-8899");
        await Expect(details.Audit).ToContainTextAsync($"UTC by {E2EFixture.AdminFullName}");
    }

    [Fact]
    public async Task AdministratorCanUploadAndRemoveALogoAndPhotos()
    {
        // Arrange
        var (context, details, _) = await OpenNewCompanyAsync();
        await using var __ = context;
        await Expect(details.LogoPlaceholder).ToBeVisibleAsync();

        // Act
        await details.UploadLogoAsync();
        await Expect(details.Message).ToHaveTextAsync("Logo saved.");
        await CompanyDetailsPage.WaitUntilShownAsync(details.LogoImage);

        await details.UploadPhotoAsync("office.png");
        await Expect(details.Message).ToHaveTextAsync("Photo added.");
        await CompanyDetailsPage.WaitUntilShownAsync(details.Photos.Locator("img"));

        await details.DeleteFirstPhotoAsync();
        await Expect(details.Photos).ToHaveCountAsync(0);
        await details.RemoveLogoAsync();

        // Assert
        await Expect(details.Message).ToHaveTextAsync("Image deleted.");
        await Expect(details.LogoPlaceholder).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AFileThatIsNotAnImageIsRefused()
    {
        // Arrange
        var (context, details, _) = await OpenNewCompanyAsync();
        await using var __ = context;

        // Act
        await details.PhotoFile.SetInputFilesAsync(new FilePayload { Name = "fake.png", MimeType = "image/png", Buffer = "not an image"u8.ToArray() });
        await details.PhotoFile.Page.GetByTestId("photo-upload").ClickAsync();

        // Assert
        await Expect(details.Error).ToHaveTextAsync("Only JPEG, PNG and WebP images are supported.");
        await Expect(details.Photos).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task AdministratorCanAddAndDeleteContactDetails()
    {
        // Arrange
        var (context, details, _) = await OpenNewCompanyAsync();
        await using var __ = context;

        // Act
        await details.AddAddressAsync("Main Street 1");
        await Expect(details.AddressRows).ToContainTextAsync("Main Street 1");
        await details.AddValueAsync("email", "info@example.com");
        await Expect(details.Rows("email")).ToContainTextAsync("info@example.com");
        await details.AddValueAsync("phone", "+46 70 123 45 67");
        await Expect(details.Rows("phone")).ToContainTextAsync("+46 70 123 45 67");

        await details.DeleteFirstAsync("address");
        await Expect(details.AddressRows).ToHaveCountAsync(0);
        await details.DeleteFirstAsync("email");
        await Expect(details.Rows("email")).ToHaveCountAsync(0);
        await details.DeleteFirstAsync("phone");

        // Assert
        await Expect(details.Message).ToHaveTextAsync("Phone number deleted.");
        await Expect(details.Rows("phone")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task DeletingFromTheDetailPageReturnsToTheList()
    {
        // Arrange
        var (context, details, name) = await OpenNewCompanyAsync();
        await using var _ = context;

        // Act
        await details.DeleteButton.ClickAsync();

        // Assert
        var companies = new CompaniesPage(details.Heading.Page);
        await Expect(companies.Message).ToHaveTextAsync("Company deleted.");
        await Expect(companies.Row(name)).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task TheSystemCompanyListsItsUsersAndCannotBeDeleted()
    {
        // Arrange
        var (context, page) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;

        // Act
        var details = await OpenAsync(page, "Test AB");

        // Assert
        await Expect(details.Users).ToContainTextAsync(E2EFixture.AdminUsername);
        await Expect(details.EditButton).ToBeVisibleAsync();
        await Expect(details.DeleteButton).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task NonAdministratorSeesTheCompanyButCannotChangeIt()
    {
        // Arrange
        var username = await fixture.CreateUserAsync("Nurse");
        var (context, page) = await SignInAsync(username, E2EFixture.UserPassword);
        await using var _ = context;

        // Act
        var details = await OpenAsync(page, "Test AB");

        // Assert
        await Expect(details.Audit).ToBeVisibleAsync();
        await Expect(details.EditButton).ToHaveCountAsync(0);
        await Expect(details.LogoFile).ToHaveCountAsync(0);
        await Expect(details.PhotoFile).ToHaveCountAsync(0);
        await Expect(details.AddButton("address")).ToHaveCountAsync(0);
        await Expect(details.AddButton("email")).ToHaveCountAsync(0);
        await Expect(details.Users).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task AnUnknownCompanyIsNotFound()
    {
        // Arrange
        var (context, page) = await SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await using var _ = context;

        // Act
        var response = await page.GotoAsync($"/Companies/Details/{Guid.NewGuid()}");

        // Assert
        response.ShouldNotBeNull().Status.ShouldBe(404);
    }
}
