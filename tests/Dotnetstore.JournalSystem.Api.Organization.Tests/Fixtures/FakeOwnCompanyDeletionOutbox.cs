using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;

/// <summary>Deletes without a message bus and remembers what would have been published.</summary>
internal sealed class FakeOwnCompanyDeletionOutbox(OrganizationDataContext context) : IOwnCompanyDeletionOutbox
{
    private readonly List<Guid> _published = [];

    internal IReadOnlyList<Guid> Published => _published;

    public async Task DeleteAndPublishAsync(Guid ownCompanyId, CancellationToken cancellationToken = default)
    {
        await context.OwnCompanies.Where(x => x.Id == ownCompanyId).ExecuteDeleteAsync(cancellationToken);
        _published.Add(ownCompanyId);
    }
}
