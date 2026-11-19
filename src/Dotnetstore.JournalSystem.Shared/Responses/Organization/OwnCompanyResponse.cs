namespace Dotnetstore.JournalSystem.Shared.Responses.Organization;

public sealed record OwnCompanyResponse(
    Guid Id,
    string Name,
    string? Description,
    Uri? LogoUrl,
    Uri? WebsiteUrl,
    string? CorporateNumber,
    string? TaxId);