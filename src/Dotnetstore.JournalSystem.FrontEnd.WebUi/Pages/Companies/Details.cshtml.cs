using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Contact;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Companies;

public sealed record CompanyUser(string Name, string Username, string Role, bool IsLockedOut);

public sealed partial class DetailsModel(
    IOwnCompanyClient ownCompanies,
    IAddressClient addresses,
    IAddressTypeClient addressTypes,
    IEmailAddressClient emailAddresses,
    IEmailAddressTypeClient emailAddressTypes,
    IPhoneNumberClient phoneNumbers,
    IPhoneNumberTypeClient phoneNumberTypes,
    IUserClient users,
    IRoleClient roles,
    ICurrentUserProfile currentProfile) : PageModel
{
    public OwnCompanyResponse? Company { get; private set; }

    public bool CanEdit { get; private set; }

    public bool CanDelete { get; private set; }

    public bool CanViewContacts { get; private set; }

    public bool CanAddContacts { get; private set; }

    public bool CanEditContacts { get; private set; }

    public bool CanDeleteContacts { get; private set; }

    public bool CanViewUsers { get; private set; }

    /// <summary>Null when the users could not be loaded. Only loaded for those who may view users.</summary>
    public IReadOnlyList<CompanyUser>? Users { get; private set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var rights = await currentProfile.GetRightsAsync(User, cancellationToken).ConfigureAwait(false);
        CanEdit = rights.Has(Permissions.OwnCompanies.Edit);
        CanDelete = rights.Has(Permissions.OwnCompanies.Delete);
        CanViewContacts = rights.Has(Permissions.ContactDetails.View);
        CanAddContacts = rights.Has(Permissions.ContactDetails.Create);
        CanEditContacts = rights.Has(Permissions.ContactDetails.Edit);
        CanDeleteContacts = rights.Has(Permissions.ContactDetails.Delete);
        CanViewUsers = rights.Has(Permissions.Users.View);

        var company = await TryAsync(() => ownCompanies.GetByIdAsync(id, cancellationToken)).ConfigureAwait(false);
        if (company is { IsForbidden: true })
        {
            return RedirectToPage("/Companies/Index");
        }

        if (company is { IsNotFound: true })
        {
            return NotFound();
        }

        Company = company is { IsSuccess: true } ? company.Value : null;
        if (Company is null)
        {
            return Page();
        }

        await LoadImagesAsync(id, cancellationToken).ConfigureAwait(false);
        if (CanViewContacts)
        {
            await LoadContactsAsync(id, cancellationToken).ConfigureAwait(false);
        }

        if (CanViewUsers)
        {
            await LoadUsersAsync(id, cancellationToken).ConfigureAwait(false);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid id, CompanyInput input, CancellationToken cancellationToken)
    {
        var request = CompanyForm.ToRequest(input, out var error);
        if (request is null)
        {
            return Refuse(id, error);
        }

        return await DecideAsync(
                id,
                async () => await ownCompanies.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false),
                "Company updated.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var outcome = await ApiAction.RunAsync(() => ownCompanies.DeleteAsync(id, cancellationToken), "Company deleted.")
            .ConfigureAwait(false);
        Message = outcome.Message;
        ErrorMessage = outcome.Error;

        return outcome.Error is null ? RedirectToPage("/Companies/Index") : RedirectToPage(new { id });
    }

    private async Task LoadUsersAsync(Guid id, CancellationToken cancellationToken)
    {
        var allUsers = await LoadListAsync(() => users.GetAllAsync(cancellationToken)).ConfigureAwait(false);
        var allRoles = await LoadListAsync(() => roles.GetAllAsync(cancellationToken)).ConfigureAwait(false);
        if (allUsers is null || allRoles is null)
        {
            return;
        }

        var roleNames = allRoles.ToDictionary(x => x.Id, x => x.Name);
        Users = allUsers
            .Where(x => x.OwnCompanyId == id)
            .Select(x => new CompanyUser(
                $"{x.FirstName} {x.LastName}",
                x.Username,
                roleNames.GetValueOrDefault(x.RoleId, "Unknown"),
                x.IsLockedOut))
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // Each section of the page degrades on its own, so a failing call gives null instead of an exception.
    private static async Task<TResult?> TryAsync<TResult>(Func<Task<TResult>> call)
        where TResult : ApiResult
    {
        try
        {
            return await call().ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TimeoutException)
        {
            return null;
        }
    }

    private static async Task<IReadOnlyList<T>?> LoadListAsync<T>(Func<Task<ApiResult<IReadOnlyList<T>>>> call)
    {
        var result = await TryAsync(call).ConfigureAwait(false);
        return result is { IsSuccess: true } ? result.Value : null;
    }

    private RedirectToPageResult Refuse(Guid id, string? error)
    {
        ErrorMessage = error;
        return RedirectToPage(new { id });
    }

    private async Task<IActionResult> DecideAsync(Guid id, Func<Task<ApiResult>> call, string success)
    {
        var outcome = await ApiAction.RunAsync(call, success).ConfigureAwait(false);
        Message = outcome.Message;
        ErrorMessage = outcome.Error;

        return RedirectToPage(new { id });
    }
}
