namespace Dotnetstore.JournalSystem.Shared.Clients;

public interface ICrudClient<in TCreateRequest, in TUpdateRequest, TResponse>
    where TCreateRequest : class
    where TUpdateRequest : class
    where TResponse : class
{
    Task<ApiResult<IReadOnlyList<TResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ApiResult<TResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResult<TResponse>> CreateAsync(TCreateRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<TResponse>> UpdateAsync(
        Guid id,
        TUpdateRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
