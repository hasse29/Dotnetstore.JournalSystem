using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Profile;

public sealed class IndexModel(ICurrentUserProfile profiles) : PageModel
{
    public MyProfileResponse? Profile { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Profile = await profiles.GetAsync(User, cancellationToken).ConfigureAwait(false);
        return Page();
    }
}
