using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.SharedKernel.Services;

public class GenericRepository<T>(DbContext context) : IGenericRepository<T>
    where T : class
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async ValueTask<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public virtual async ValueTask<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FindAsync([id], cancellationToken)
            .ConfigureAwait(false);
    }

    public virtual async ValueTask AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);
    }

    public virtual async ValueTask AddAsync(IReadOnlyCollection<T> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet
            .AddRangeAsync(entities, cancellationToken)
            .ConfigureAwait(false);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual void Update(IReadOnlyCollection<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public virtual async ValueTask DeleteAsync(object id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (entity is not null)
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual async ValueTask DeleteAsync(IReadOnlyCollection<object> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);

        foreach (var id in ids)
        {
            await DeleteAsync(id, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public virtual void Detach(T entity)
    {
        context.Entry(entity).State = EntityState.Detached;
    }
}
