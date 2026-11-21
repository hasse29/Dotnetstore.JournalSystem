using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Access;

public sealed class StatusModel(IAccessRequestClient requests) : PageModel
{
    public AccessRequestStatus? State { get; private set; }

    public string? DecisionNote { get; private set; }

    public bool Unavailable { get; private set; }

    // Visiting this page is what files the request, so signing in with Microsoft is all a newcomer has to do.
    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        ApiResult<AccessRequestResponse> result;
        try
        {
            result = await requests.RequestMyAccessAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            Unavailable = true;
            return Page();
        }
        catch (TimeoutException)
        {
            Unavailable = true;
            return Page();
        }

        if (result.IsUnauthorized)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            return RedirectToPage("/Account/Login", new { error = "unauthorized" });
        }

        if (result.IsForbidden)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        if (result is { IsSuccess: true, Value: { } request })
        {
            if (request.Status == AccessRequestStatus.Approved)
            {
                return RedirectToPage("/Dashboard/Index");
            }

            State = request.Status;
            DecisionNote = request.DecisionNote;
            return Page();
        }

        Unavailable = true;
        return Page();
    }
}
