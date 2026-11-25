using Dotnetstore.JournalSystem.Shared.Requests.Organization;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Companies;

public sealed class CompanyInput
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? WebsiteUrl { get; set; }

    public string? LogoUrl { get; set; }

    public string? CorporateNumber { get; set; }

    public string? TaxId { get; set; }
}

/// <summary>Turns the company form into an API request; shared by the list and the detail page.</summary>
internal static class CompanyForm
{
    internal static EditOwnCompanyRequest? ToRequest(CompanyInput? input, out string? error)
    {
        ArgumentNullException.ThrowIfNull(input);
        error = null;

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            error = "A company needs a name.";
            return null;
        }

        if (!TryParseWebAddress(input.WebsiteUrl, out var website))
        {
            error = "The website must be a full http or https address.";
            return null;
        }

        // The logo address is no longer typed in; a stored value is only carried through an edit.
        _ = Uri.TryCreate(input.LogoUrl, UriKind.Absolute, out var logo);

        return new EditOwnCompanyRequest(
            input.Name.Trim(),
            NullIfBlank(input.Description),
            logo,
            website,
            NullIfBlank(input.CorporateNumber),
            NullIfBlank(input.TaxId));
    }

    internal static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool TryParseWebAddress(string? value, out Uri? uri)
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
}
