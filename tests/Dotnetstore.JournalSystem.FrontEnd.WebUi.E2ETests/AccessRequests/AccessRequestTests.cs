using System.Text.RegularExpressions;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;
using Microsoft.Playwright;
using Shouldly;
using static Microsoft.Playwright.Assertions;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.AccessRequests;

[Collection(MicrosoftSignInSerial.Name)]
public sealed class AccessRequestTests(E2EFixture fixture)
{
    private const string AdministratorRole = "Administrator";

    private async Task SignInAsPersonAsync(IPage page, FakePerson person)
    {
        var login = new LoginPage(page);
        await login.GotoAsync();
        fixture.Idp.NextPerson = person;
        await login.MicrosoftButton.ClickAsync();
    }

    private static async Task SignInAsAdministratorAsync(IPage page)
    {
        var login = new LoginPage(page);
        await login.GotoAsync();
        await login.SignInAsync(E2EFixture.AdminUsername, E2EFixture.AdminPassword);
        await Expect(new DashboardPage(page).Heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task UnknownMicrosoftUserIsToldTheirRequestIsWaitingForApproval()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var status = new AccessStatusPage(page);

        // Act
        await SignInAsPersonAsync(page, FakePerson.CreateUnknown());

        // Assert
        await Expect(page).ToHaveURLAsync(new Regex("/Access/Status$"));
        await Expect(status.Pending).ToBeVisibleAsync();
    }

    [Fact]
    public async Task UnapprovedUserIsKeptOutOfTheDashboardAndTheAdminPage()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var status = new AccessStatusPage(page);
        var dashboard = new DashboardPage(page);
        await SignInAsPersonAsync(page, FakePerson.CreateUnknown());
        await Expect(status.Pending).ToBeVisibleAsync();

        // Act
        await dashboard.GotoAsync();

        // Assert
        await Expect(page).ToHaveURLAsync(new Regex("/Access/Status$"));
        await Expect(dashboard.Heading).ToBeHiddenAsync();

        // Act
        await page.GotoAsync("/Admin/AccessRequests");

        // Assert
        await Expect(page).ToHaveURLAsync(new Regex("/Access/Status$"));
    }

    [Fact]
    public async Task UnapprovedUserCanSignOutFromTheStatusPage()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var status = new AccessStatusPage(page);
        await SignInAsPersonAsync(page, FakePerson.CreateUnknown());
        await Expect(status.Pending).ToBeVisibleAsync();

        // Act
        await status.LogoutButton.ClickAsync();

        // Assert
        await Expect(new LoginPage(page).Heading).ToBeVisibleAsync();
        await page.GotoAsync("/Access/Status");
        await Expect(new LoginPage(page).Heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AdministratorApprovesARequestAndThePersonCanThenUseTheSystem()
    {
        // Arrange
        var person = FakePerson.CreateUnknown();
        await using var personContext = await fixture.NewContextAsync();
        var personPage = await personContext.NewPageAsync();
        await SignInAsPersonAsync(personPage, person);
        await Expect(new AccessStatusPage(personPage).Pending).ToBeVisibleAsync();

        await using var adminContext = await fixture.NewContextAsync();
        var adminPage = await adminContext.NewPageAsync();
        await SignInAsAdministratorAsync(adminPage);
        var requests = new AccessRequestsPage(adminPage);
        await requests.GotoAsync();
        await Expect(requests.Pending(person.UserName)).ToBeVisibleAsync();

        // Act
        await requests.ApproveAsync(person.UserName, AdministratorRole);

        // Assert
        await Expect(requests.Message).ToHaveTextAsync("Access approved.");
        await Expect(requests.Pending(person.UserName)).ToBeHiddenAsync();
        await Expect(requests.Decided(person.UserName)).ToContainTextAsync("Approved");

        await using var returningContext = await fixture.NewContextAsync();
        var returningPage = await returningContext.NewPageAsync();
        await SignInAsPersonAsync(returningPage, person);
        var dashboard = new DashboardPage(returningPage);
        await Expect(returningPage).ToHaveURLAsync(new Regex("/Dashboard$"));
        await Expect(dashboard.Heading).ToBeVisibleAsync();
        await Expect(dashboard.StatValue("roles")).ToHaveTextAsync(new Regex("^[0-9]+$"));
    }

    [Fact]
    public async Task ApprovalTakesEffectForAPersonAlreadyWaitingOnTheStatusPage()
    {
        // Arrange
        var person = FakePerson.CreateUnknown();
        await using var personContext = await fixture.NewContextAsync();
        var personPage = await personContext.NewPageAsync();
        await SignInAsPersonAsync(personPage, person);
        await Expect(new AccessStatusPage(personPage).Pending).ToBeVisibleAsync();

        await using var adminContext = await fixture.NewContextAsync();
        var adminPage = await adminContext.NewPageAsync();
        await SignInAsAdministratorAsync(adminPage);
        var requests = new AccessRequestsPage(adminPage);
        await requests.GotoAsync();
        await requests.ApproveAsync(person.UserName, AdministratorRole);
        await Expect(requests.Message).ToBeVisibleAsync();

        // Act
        await personPage.GotoAsync("/Dashboard");

        // Assert
        await Expect(new DashboardPage(personPage).Heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task RejectedPersonSeesTheReasonAndCanBeLetInAfterTheRequestIsReopened()
    {
        // Arrange
        var person = FakePerson.CreateUnknown();
        await using var personContext = await fixture.NewContextAsync();
        var personPage = await personContext.NewPageAsync();
        await SignInAsPersonAsync(personPage, person);
        await Expect(new AccessStatusPage(personPage).Pending).ToBeVisibleAsync();

        await using var adminContext = await fixture.NewContextAsync();
        var adminPage = await adminContext.NewPageAsync();
        await SignInAsAdministratorAsync(adminPage);
        var requests = new AccessRequestsPage(adminPage);
        await requests.GotoAsync();

        // Act
        await requests.RejectAsync(person.UserName, "Not part of the clinic");

        // Assert
        await Expect(requests.Message).ToHaveTextAsync("Request rejected.");
        await Expect(requests.Decided(person.UserName)).ToContainTextAsync("Rejected");

        await using var rejectedContext = await fixture.NewContextAsync();
        var rejectedPage = await rejectedContext.NewPageAsync();
        await SignInAsPersonAsync(rejectedPage, person);
        var status = new AccessStatusPage(rejectedPage);
        await Expect(status.Rejected).ToBeVisibleAsync();
        await Expect(status.Note).ToHaveTextAsync("Not part of the clinic");
        await rejectedPage.GotoAsync("/Dashboard");
        await Expect(new DashboardPage(rejectedPage).Heading).ToBeHiddenAsync();

        // Act
        await requests.ReopenAsync(person.UserName);

        // Assert
        await Expect(requests.Message).ToHaveTextAsync("Request reopened.");
        await Expect(requests.Pending(person.UserName)).ToBeVisibleAsync();

        await using var reopenedContext = await fixture.NewContextAsync();
        var reopenedPage = await reopenedContext.NewPageAsync();
        await SignInAsPersonAsync(reopenedPage, person);
        await Expect(new AccessStatusPage(reopenedPage).Pending).ToBeVisibleAsync();
    }

    [Fact]
    public async Task AdministratorSeesTheAccessRequestsLink()
    {
        // Arrange
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        await SignInAsAdministratorAsync(page);
        var requests = new AccessRequestsPage(page);

        // Act
        await requests.NavLink.ClickAsync();

        // Assert
        await Expect(page).ToHaveURLAsync(new Regex("/Admin/AccessRequests$"));
        await Expect(requests.Heading).ToBeVisibleAsync();
    }

    [Fact]
    public async Task OrdinaryUserDoesNotSeeTheLinkAndIsToldTheyLackPermissionOnThePage()
    {
        // Arrange
        var username = await fixture.CreateUserAsync("Nurse");
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var login = new LoginPage(page);
        var requests = new AccessRequestsPage(page);
        await login.GotoAsync();
        await login.SignInAsync(username, E2EFixture.UserPassword);
        await Expect(new DashboardPage(page).Heading).ToBeVisibleAsync();

        // Act & Assert
        await Expect(requests.NavLink).ToHaveCountAsync(0);

        // Act
        await requests.GotoAsync();

        // Assert
        await Expect(requests.Forbidden).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ApprovalFormIsPrefilledFromTheMicrosoftNameAndRequiresAGender()
    {
        // Arrange
        var person = new FakePerson(Guid.NewGuid().ToString("D"), "Nora Newcomer", $"nora.{Guid.NewGuid():N}@example.com");
        await using var personContext = await fixture.NewContextAsync();
        var personPage = await personContext.NewPageAsync();
        await SignInAsPersonAsync(personPage, person);
        await Expect(new AccessStatusPage(personPage).Pending).ToBeVisibleAsync();

        await using var adminContext = await fixture.NewContextAsync();
        var adminPage = await adminContext.NewPageAsync();
        await SignInAsAdministratorAsync(adminPage);
        var requests = new AccessRequestsPage(adminPage);

        // Act
        await requests.GotoAsync();

        // Assert
        await Expect(requests.FirstNameInput(person.UserName)).ToHaveValueAsync("Nora");
        await Expect(requests.LastNameInput(person.UserName)).ToHaveValueAsync("Newcomer");
        await Expect(requests.Pending(person.UserName).GetByTestId("gender")).ToHaveValueAsync(string.Empty);
    }

    [Fact]
    public async Task ApprovedPersonIsIdentifiedByNameRoleAndCompanyWithTheIdNumberMasked()
    {
        // Arrange
        var person = FakePerson.CreateUnknown();
        await using var personContext = await fixture.NewContextAsync();
        var personPage = await personContext.NewPageAsync();
        await SignInAsPersonAsync(personPage, person);
        await Expect(new AccessStatusPage(personPage).Pending).ToBeVisibleAsync();

        await using var adminContext = await fixture.NewContextAsync();
        var adminPage = await adminContext.NewPageAsync();
        await SignInAsAdministratorAsync(adminPage);
        var requests = new AccessRequestsPage(adminPage);
        await requests.GotoAsync();
        await requests.FirstNameInput(person.UserName).FillAsync("Nora");
        await requests.LastNameInput(person.UserName).FillAsync("Newcomer");
        await requests.ApproveAsync(person.UserName, AdministratorRole, "female", "1990-01-01", "19900101-1234");
        await Expect(requests.Message).ToHaveTextAsync("Access approved.");

        // Act
        await using var returningContext = await fixture.NewContextAsync();
        var returningPage = await returningContext.NewPageAsync();
        await SignInAsPersonAsync(returningPage, person);
        var dashboard = new DashboardPage(returningPage);
        var profile = new ProfilePage(returningPage);
        await Expect(dashboard.Heading).ToBeVisibleAsync();

        // Assert
        await Expect(dashboard.CurrentUser).ToHaveTextAsync("Nora Newcomer");
        await Expect(dashboard.CurrentUserDetail).ToContainTextAsync("Test AB");
        await profile.NavLink.ClickAsync();
        await Expect(profile.Heading).ToBeVisibleAsync();
        await Expect(profile.Name).ToHaveTextAsync("Nora Newcomer");
        await Expect(profile.Provider).ToHaveTextAsync("Microsoft");
        await Expect(profile.Company).ToHaveTextAsync("Test AB");
        await Expect(profile.Gender).ToHaveTextAsync("Female");
        await Expect(profile.DateOfBirth).ToHaveTextAsync("1990-01-01");
        await Expect(profile.PersonalId).ToHaveTextAsync("19900101-****");
        (await returningPage.ContentAsync()).ShouldNotContain("199001011234");
    }

    [Fact]
    public async Task ApprovingWithAMalformedIdNumberKeepsTheRequestPendingAndShowsTheError()
    {
        // Arrange
        var person = FakePerson.CreateUnknown();
        await using var personContext = await fixture.NewContextAsync();
        var personPage = await personContext.NewPageAsync();
        await SignInAsPersonAsync(personPage, person);
        await Expect(new AccessStatusPage(personPage).Pending).ToBeVisibleAsync();

        await using var adminContext = await fixture.NewContextAsync();
        var adminPage = await adminContext.NewPageAsync();
        await SignInAsAdministratorAsync(adminPage);
        var requests = new AccessRequestsPage(adminPage);
        await requests.GotoAsync();

        // Act
        await requests.ApproveAsync(person.UserName, AdministratorRole, "male", null, "123");

        // Assert
        await Expect(requests.Error).ToBeVisibleAsync();
        await Expect(requests.Pending(person.UserName)).ToBeVisibleAsync();
    }
}
