using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.AccessRequests;

internal interface IAccessRequestService
{
    ValueTask<Result<AccessRequestResponse>> RequestAccessAsync(EntraIdentity identity, CancellationToken cancellationToken = default);

    ValueTask<Result<AccessRequestResponse>> GetMineAsync(string externalId, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<AccessRequestResponse>> GetAllAsync(AccessRequestStatus? status, CancellationToken cancellationToken = default);

    ValueTask<Result<AccessRequestResponse>> ApproveAsync(Guid id, ApproveAccessRequestRequest request, Guid? decidedBy, CancellationToken cancellationToken = default);

    ValueTask<Result<AccessRequestResponse>> RejectAsync(Guid id, RejectAccessRequestRequest request, Guid? decidedBy, CancellationToken cancellationToken = default);

    ValueTask<Result<AccessRequestResponse>> ReopenAsync(Guid id, CancellationToken cancellationToken = default);
}
