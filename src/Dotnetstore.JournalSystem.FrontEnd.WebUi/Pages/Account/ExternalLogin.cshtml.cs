using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Account;

[AllowAnonymous]
public sealed class ExternalLoginModel(EntraSignInSettings entra) : PageModel
{
    public IActionResult OnGet(string? returnUrl)
    {
        if (!entra.Enabled)
        {
            return NotFound();
        }

        var target = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/Dashboard";
        return Challenge(new AuthenticationProperties { RedirectUri = target }, EntraSignInSettings.SchemeName);
    }
}
