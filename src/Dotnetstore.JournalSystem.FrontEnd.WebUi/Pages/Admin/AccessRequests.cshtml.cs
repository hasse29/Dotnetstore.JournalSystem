using System.Globalization;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Admin;

public sealed class AccessRequestsModel(
    IAccessRequestClient requests,
    IRoleClient roles,
    IOwnCompanyClient ownCompanies) : PageModel
{
    public IReadOnlyList<AccessRequestResponse> Pending { get; private set; } = [];

    public IReadOnlyList<AccessRequestResponse> Decided { get; private set; } = [];

    public IReadOnlyList<SelectListItem> Roles { get; private set; } = [];

    public IReadOnlyList<SelectListItem> Companies { get; private set; } = [];

    public bool Forbidden { get; private set; }

    public bool Unavailable { get; private set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var requestsTask = requests.GetAllAsync(null, cancellationToken);
            var rolesTask = roles.GetAllAsync(cancellationToken);
            var companiesTask = ownCompanies.GetAllAsync(cancellationToken);
            await Task.WhenAll(requestsTask, rolesTask, companiesTask).ConfigureAwait(false);

            var list = await requestsTask.ConfigureAwait(false);
            if (list.IsForbidden)
            {
                Forbidden = true;
                return Page();
            }

            if (!list.IsSuccess || list.Value is null)
            {
                Unavailable = true;
                return Page();
            }

            Pending = list.Value.Where(x => x.Status == AccessRequestStatus.Pending).OrderBy(x => x.RequestedAt).ToList();
            Decided = list.Value.Where(x => x.Status != AccessRequestStatus.Pending).OrderByDescending(x => x.DecidedAt).ToList();

            Roles = (await rolesTask.ConfigureAwait(false)).Value?
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new SelectListItem(x.Name, x.Id.ToString("D", CultureInfo.InvariantCulture)))
                .ToList() ?? [];
            Companies = (await companiesTask.ConfigureAwait(false)).Value?
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new SelectListItem(x.Name, x.Id.ToString("D", CultureInfo.InvariantCulture)))
                .ToList() ?? [];
        }
        catch (HttpRequestException)
        {
            Unavailable = true;
        }
        catch (TimeoutException)
        {
            Unavailable = true;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id, Guid? ownCompanyId, Guid? roleId, CancellationToken cancellationToken)
    {
        if (ownCompanyId is null || roleId is null)
        {
            ErrorMessage = "Choose a role and a company before approving.";
            return RedirectToPage();
        }

        return await DecideAsync(
                () => requests.ApproveAsync(id, new ApproveAccessRequestRequest(ownCompanyId.Value, roleId.Value), cancellationToken),
                "Access approved.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id, string? note, CancellationToken cancellationToken) =>
        await DecideAsync(
                () => requests.RejectAsync(id, new RejectAccessRequestRequest(note), cancellationToken),
                "Request rejected.")
            .ConfigureAwait(false);

    public async Task<IActionResult> OnPostReopenAsync(Guid id, CancellationToken cancellationToken) =>
        await DecideAsync(
                () => requests.ReopenAsync(id, cancellationToken),
                "Request reopened.")
            .ConfigureAwait(false);

    private async Task<IActionResult> DecideAsync(Func<Task<ApiResult<AccessRequestResponse>>> call, string success)
    {
        try
        {
            var result = await call().ConfigureAwait(false);

            if (result.IsSuccess)
            {
                Message = success;
            }
            else if (result.IsForbidden)
            {
                ErrorMessage = "You do not have permission to decide access requests.";
            }
            else if (result.IsNotFound)
            {
                ErrorMessage = "That request no longer exists.";
            }
            else
            {
                ErrorMessage = result.Problem.Detail ?? result.Problem.Errors.Values.SelectMany(x => x).FirstOrDefault() ?? "The request could not be updated.";
            }
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "The service is unavailable right now. Please try again.";
        }
        catch (TimeoutException)
        {
            ErrorMessage = "The service did not respond in time. Please try again.";
        }

        return RedirectToPage();
    }
}
