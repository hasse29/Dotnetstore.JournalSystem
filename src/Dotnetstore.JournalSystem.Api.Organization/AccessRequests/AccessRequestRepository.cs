using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.AccessRequests;

internal sealed class AccessRequestRepository(OrganizationDataContext dataContext)
    : GenericRepository<AccessRequest>(dataContext), IAccessRequestRepository
{
    async Task<AccessRequest?> IAccessRequestRepository.GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        return await dataContext.AccessRequests
            .FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken)
            .ConfigureAwait(false);
    }

    async Task<IReadOnlyList<AccessRequest>> IAccessRequestRepository.GetByStatusAsync(
        AccessRequestStatus? status,
        CancellationToken cancellationToken)
    {
        var query = dataContext.AccessRequests.AsNoTracking();

        if (status is { } value)
        {
            query = query.Where(x => x.Status == value);
        }

        return await query
            .OrderBy(x => x.RequestedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    async Task IAccessRequestRepository.DeleteByExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        await dataContext.AccessRequests
            .Where(x => x.ExternalId == externalId)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
