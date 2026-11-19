using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;

internal static class OwnCompanyTestData
{
    internal static readonly DateTime CreatedDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    internal static OwnCompany Create(
        Guid id,
        string name,
        string? description = null,
        Uri? logoUrl = null,
        Uri? websiteUrl = null,
        string? corporateNumber = null,
        string? taxId = null)
    {
        return new OwnCompany(
            id,
            name,
            description,
            logoUrl,
            websiteUrl,
            corporateNumber,
            taxId,
            CreatedDate,
            null,
            null,
            null,
            null,
            null,
            false,
            false,
            false)
        {
            Name = name
        };
    }
}
