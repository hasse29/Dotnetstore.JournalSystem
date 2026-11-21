using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Api.Organization.AccessRequests;

internal interface IAccessRequestRepository : IGenericRepository<AccessRequest>
{
    Task<AccessRequest?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AccessRequest>> GetByStatusAsync(AccessRequestStatus? status, CancellationToken cancellationToken = default);

    Task DeleteByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
}
