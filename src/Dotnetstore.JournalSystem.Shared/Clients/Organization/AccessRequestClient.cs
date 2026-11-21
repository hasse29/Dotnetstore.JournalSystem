using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Shared.Clients.Organization;

public interface IAccessRequestClient
{
    Task<ApiResult<AccessRequestResponse>> RequestMyAccessAsync(CancellationToken cancellationToken = default);

    Task<ApiResult<AccessRequestResponse>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<ApiResult<AccessRequestResponse[]>> GetAllAsync(AccessRequestStatus? status = null, CancellationToken cancellationToken = default);

    Task<ApiResult<AccessRequestResponse>> ApproveAsync(Guid id, ApproveAccessRequestRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<AccessRequestResponse>> RejectAsync(Guid id, RejectAccessRequestRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<AccessRequestResponse>> ReopenAsync(Guid id, CancellationToken cancellationToken = default);
}

internal sealed class AccessRequestClient(HttpClient http) : ApiClientBase(http), IAccessRequestClient
{
    public Task<ApiResult<AccessRequestResponse>> RequestMyAccessAsync(CancellationToken cancellationToken = default) =>
        SendAsync<AccessRequestResponse>(HttpMethod.Post, BuildUri(ApiEndpoints.Organization.AccessRequests.Me), null, cancellationToken);

    public Task<ApiResult<AccessRequestResponse>> GetMineAsync(CancellationToken cancellationToken = default) =>
        SendAsync<AccessRequestResponse>(HttpMethod.Get, BuildUri(ApiEndpoints.Organization.AccessRequests.Me), null, cancellationToken);

    public Task<ApiResult<AccessRequestResponse[]>> GetAllAsync(
        AccessRequestStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var path = status is { } value
            ? $"{ApiEndpoints.Organization.AccessRequests.GetAll}?status={value}"
            : ApiEndpoints.Organization.AccessRequests.GetAll;

        return SendAsync<AccessRequestResponse[]>(HttpMethod.Get, BuildUri(path), null, cancellationToken);
    }

    public Task<ApiResult<AccessRequestResponse>> ApproveAsync(
        Guid id,
        ApproveAccessRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync<AccessRequestResponse>(HttpMethod.Post, BuildUri(ApiEndpoints.Organization.AccessRequests.Approve, id), request, cancellationToken);
    }

    public Task<ApiResult<AccessRequestResponse>> RejectAsync(
        Guid id,
        RejectAccessRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync<AccessRequestResponse>(HttpMethod.Post, BuildUri(ApiEndpoints.Organization.AccessRequests.Reject, id), request, cancellationToken);
    }

    public Task<ApiResult<AccessRequestResponse>> ReopenAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<AccessRequestResponse>(HttpMethod.Post, BuildUri(ApiEndpoints.Organization.AccessRequests.Reopen, id), null, cancellationToken);
}
