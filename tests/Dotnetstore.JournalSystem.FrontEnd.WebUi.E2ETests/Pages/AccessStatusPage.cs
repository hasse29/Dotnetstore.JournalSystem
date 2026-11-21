using Microsoft.Playwright;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Pages;

internal sealed class AccessStatusPage(IPage page)
{
    public ILocator Pending => page.GetByTestId("access-pending");

    public ILocator Rejected => page.GetByTestId("access-rejected");

    public ILocator Note => page.GetByTestId("access-note");

    public ILocator LogoutButton => page.GetByTestId("access-logout");
}
