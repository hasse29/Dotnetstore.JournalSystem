using Dotnetstore.JournalSystem.Shared.Requests.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Seeding;

internal sealed class OrganizationSeedData
{
    public List<EditOwnCompanyRequest> OwnCompanies { get; init; } = [];
}
