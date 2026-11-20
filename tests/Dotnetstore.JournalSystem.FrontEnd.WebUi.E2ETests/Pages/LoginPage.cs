using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;

internal sealed class LoginPage(IPage page)
{
    public ILocator Heading => page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Sign in", Level = 1 });

    public ILocator UsernameInput => page.GetByLabel("Username");

    public ILocator PasswordInput => page.GetByLabel("Password");

    public ILocator SubmitButton => page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in" });

    public ILocator Error => page.GetByTestId("login-error");

    public ILocator MicrosoftButton => page.GetByTestId("microsoft-signin");

    public async Task GotoAsync(string path = "/Account/Login") => await page.GotoAsync(path);

    public async Task SignInAsync(string username, string password)
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        await SubmitButton.ClickAsync();
    }

    public static Regex UrlPattern() => new(@"/Account/Login", RegexOptions.None, TimeSpan.FromSeconds(1));
}
