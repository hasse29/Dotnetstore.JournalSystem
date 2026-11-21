using Microsoft.Playwright;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;

internal sealed class ProfilePage(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "My profile", Level = 1 });

    public ILocator NavLink => page.GetByTestId("nav-profile");

    public ILocator Name => page.GetByTestId("profile-name");

    public ILocator Username => page.GetByTestId("profile-username");

    public ILocator Provider => page.GetByTestId("profile-provider");

    public ILocator Company => page.GetByTestId("profile-company");

    public ILocator Role => page.GetByTestId("profile-role");

    public ILocator Gender => page.GetByTestId("profile-gender");

    public ILocator DateOfBirth => page.GetByTestId("profile-dob");

    public ILocator PersonalId => page.GetByTestId("profile-personal-id");

    public async Task GotoAsync() => await page.GotoAsync("/Profile");
}
