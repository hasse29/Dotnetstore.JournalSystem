using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Companies;

public sealed class IndexModel(IOwnCompanyClient ownCompanies) : PageModel
{
    public IReadOnlyList<OwnCompanyResponse> Companies { get; private set; } = [];

    public bool Unavailable { get; private set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await ownCompanies.GetAllAsync(cancellationToken).ConfigureAwait(false);

            if (result.IsSuccess && result.Value is not null)
            {
                Companies = result.Value.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
            }
            else
            {
                Unavailable = true;
            }
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

    public async Task<IActionResult> OnPostCreateAsync(CompanyInput input, CancellationToken cancellationToken)
    {
        var request = ToRequest(input);
        if (request is null)
        {
            return RedirectToPage();
        }

        return await DecideAsync(
                async () => await ownCompanies.CreateAsync(request, cancellationToken).ConfigureAwait(false),
                "Company created.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid id, CompanyInput input, CancellationToken cancellationToken)
    {
        var request = ToRequest(input);
        if (request is null)
        {
            return RedirectToPage();
        }

        return await DecideAsync(
                async () => await ownCompanies.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false),
                "Company updated.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken) =>
        await DecideAsync(
                async () => await ownCompanies.DeleteAsync(id, cancellationToken).ConfigureAwait(false),
                "Company deleted.")
            .ConfigureAwait(false);

    private EditOwnCompanyRequest? ToRequest(CompanyInput? input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            ErrorMessage = "A company needs a name.";
            return null;
        }

        if (!TryParseUrl(input.WebsiteUrl, out var website) || !TryParseUrl(input.LogoUrl, out var logo))
        {
            ErrorMessage = "Website and logo must be full http or https addresses.";
            return null;
        }

        return new EditOwnCompanyRequest(
            input.Name.Trim(),
            NullIfBlank(input.Description),
            logo,
            website,
            NullIfBlank(input.CorporateNumber),
            NullIfBlank(input.TaxId));
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool TryParseUrl(string? value, out Uri? uri)
    {
        uri = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (Uri.TryCreate(value.Trim(), UriKind.Absolute, out var parsed) && parsed.Scheme is "http" or "https")
        {
            uri = parsed;
            return true;
        }

        return false;
    }

    private async Task<IActionResult> DecideAsync(Func<Task<ApiResult>> call, string success)
    {
        try
        {
            var result = await call().ConfigureAwait(false);

            if (result.IsSuccess)
            {
                Message = success;
            }
            else if (result.IsForbidden || result.IsUnauthorized)
            {
                ErrorMessage = "Only administrators can change companies.";
            }
            else if (result.IsNotFound)
            {
                ErrorMessage = "That company no longer exists.";
            }
            else
            {
                ErrorMessage = result.Problem.Detail ?? result.Problem.Errors.Values.SelectMany(x => x).FirstOrDefault() ?? "The company could not be saved.";
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

public sealed class CompanyInput
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? LogoUrl { get; set; }

    public string? CorporateNumber { get; set; }

    public string? TaxId { get; set; }
}
