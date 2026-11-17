namespace Dotnetstore.JournalSystem.Api.SharedKernel.Services;

public interface IGenericRepository<T> where T : class
{
    ValueTask<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
    ValueTask AddAsync(T entity, CancellationToken cancellationToken = default);
    ValueTask AddAsync(IReadOnlyCollection<T> entities, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Update(IReadOnlyCollection<T> entities);
    ValueTask DeleteAsync(object id, CancellationToken cancellationToken = default);
    ValueTask DeleteAsync(IReadOnlyCollection<object> ids, CancellationToken cancellationToken = default);
    void Detach(T entity);
}