using System.Globalization;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
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
    IOwnCompanyClient ownCompanies,
    ICurrentUserProfile currentProfile) : PageModel
{
    internal const string GenderMale = "male";
    internal const string GenderFemale = "female";

    public IReadOnlyList<AccessRequestResponse> Pending { get; private set; } = [];

    public IReadOnlyList<AccessRequestResponse> Decided { get; private set; } = [];

    public IReadOnlyList<SelectListItem> Roles { get; private set; } = [];

    public IReadOnlyList<SelectListItem> Companies { get; private set; } = [];

    public bool Forbidden { get; private set; }

    public bool CanDecide { get; private set; }

    public bool Unavailable { get; private set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var rights = await currentProfile.GetRightsAsync(User, cancellationToken).ConfigureAwait(false);
        CanDecide = rights.Has(Permissions.AccessRequests.Edit);

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

    public async Task<IActionResult> OnPostApproveAsync(Guid id, ApprovalInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.OwnCompanyId is null || input.RoleId is null || string.IsNullOrWhiteSpace(input.FirstName) ||
            string.IsNullOrWhiteSpace(input.LastName) || input.Gender is not (GenderMale or GenderFemale))
        {
            ErrorMessage = "Fill in first name, last name, gender, company and role before approving.";
            return RedirectToPage();
        }

        DateOnly? dateOfBirth = null;
        if (!string.IsNullOrWhiteSpace(input.DateOfBirth))
        {
            if (!DateOnly.TryParseExact(input.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                ErrorMessage = "Date of birth must be a valid date.";
                return RedirectToPage();
            }

            dateOfBirth = parsed;
        }

        var request = new ApproveAccessRequestRequest(
            input.OwnCompanyId.Value,
            input.RoleId.Value,
            input.FirstName.Trim(),
            string.IsNullOrWhiteSpace(input.MiddleName) ? null : input.MiddleName.Trim(),
            input.LastName.Trim(),
            input.Gender == GenderMale,
            dateOfBirth,
            string.IsNullOrWhiteSpace(input.SocialSecurityNumber) ? null : input.SocialSecurityNumber.Trim());

        return await DecideAsync(
                () => requests.ApproveAsync(id, request, cancellationToken),
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
                ErrorMessage = result.Problem.Detail ?? "You do not have permission to decide access requests.";
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

public sealed class ApprovalInput
{
    public Guid? OwnCompanyId { get; set; }

    public Guid? RoleId { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public string? Gender { get; set; }

    public string? DateOfBirth { get; set; }

    public string? SocialSecurityNumber { get; set; }
}
