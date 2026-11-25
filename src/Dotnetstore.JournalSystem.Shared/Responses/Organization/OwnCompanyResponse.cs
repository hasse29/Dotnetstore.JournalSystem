namespace Dotnetstore.JournalSystem.Shared.Responses.Organization;

/// <param name="CreatedDate">The audit values are only filled in when a single company is fetched.</param>
public sealed record OwnCompanyResponse(
    Guid Id,
    string Name,
    string? Description,
    Uri? LogoUrl,
    Uri? WebsiteUrl,
    string? CorporateNumber,
    string? TaxId,
    bool IsSystem);
