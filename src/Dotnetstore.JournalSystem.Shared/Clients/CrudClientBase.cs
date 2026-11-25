namespace Dotnetstore.JournalSystem.Shared.Clients;

internal abstract class CrudClientBase<TCreateRequest, TUpdateRequest, TResponse>(
    HttpClient http,
    string getAll,
    string getById,
    string create,
    string update,
    string delete) : ApiClientBase(http)
    where TCreateRequest : class
    where TUpdateRequest : class
    where TResponse : class
{
    public Task<ApiResult<IReadOnlyList<TResponse>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetListAsync(BuildUri(getAll), cancellationToken);

    protected async Task<ApiResult<IReadOnlyList<TResponse>>> GetListAsync(Uri uri, CancellationToken cancellationToken)
    {
        var result = await SendAsync<List<TResponse>>(HttpMethod.Get, uri, null, cancellationToken)
            .ConfigureAwait(false);

        return ApiResult.Create<IReadOnlyList<TResponse>>(result.StatusCode, result.Value, result.Problem);
    }

    public Task<ApiResult<TResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync<TResponse>(HttpMethod.Get, BuildUri(getById, id), null, cancellationToken);

    public Task<ApiResult<TResponse>> CreateAsync(
        TCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync<TResponse>(HttpMethod.Post, BuildUri(create), request, cancellationToken);
    }

    public Task<ApiResult<TResponse>> UpdateAsync(
        Guid id,
        TUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync<TResponse>(HttpMethod.Put, BuildUri(update, id), request, cancellationToken);
    }

    public Task<ApiResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, BuildUri(delete, id), null, cancellationToken);
}
