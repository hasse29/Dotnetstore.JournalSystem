namespace Dotnetstore.JournalSystem.Shared.Requests.Organization;

public sealed record EditOwnCompanyRequest(
    string Name,
    string? Description,
    Uri? LogoUrl,
    Uri? WebsiteUrl,
    string? CorporateNumber,
    string? TaxId);