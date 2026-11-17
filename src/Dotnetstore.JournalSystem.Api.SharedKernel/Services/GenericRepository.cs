using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.SharedKernel.Services;

public sealed class GenericRepository<T>(DbContext context) : IGenericRepository<T>
    where T : class
{
    private readonly DbSet<T> _dbSet = context.Set<T>();
    
    async ValueTask<IEnumerable<T>> IGenericRepository<T>.GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbSet
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    async ValueTask<T?> IGenericRepository<T>.GetByIdAsync(object id, CancellationToken cancellationToken)
    {
        return await GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);
    }

    async ValueTask IGenericRepository<T>.AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _dbSet
            .AddAsync(entity, cancellationToken)
            .ConfigureAwait(false);
    }

    async ValueTask IGenericRepository<T>.AddAsync(IReadOnlyCollection<T> entities, CancellationToken cancellationToken)
    {
        await _dbSet
            .AddRangeAsync(entities, cancellationToken)
            .ConfigureAwait(false);
    }

    void IGenericRepository<T>.Update(T entity)
    {
        _dbSet.Update(entity);
    }

    void IGenericRepository<T>.Update(IReadOnlyCollection<T> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    async ValueTask IGenericRepository<T>.DeleteAsync(object id, CancellationToken cancellationToken)
    {
        var entity = await GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);
        
        if(entity is not null)
        {
            _dbSet.Remove(entity);
        }
    }

    async ValueTask IGenericRepository<T>.DeleteAsync(IReadOnlyCollection<object> ids, CancellationToken cancellationToken)
    {
        foreach (var id in ids)
        {
            var entity = await GetByIdAsync(id, cancellationToken)
                .ConfigureAwait(false);
        
            if(entity is not null)
            {
                _dbSet.Remove(entity);
            }
        }
    }
    
    private async ValueTask<T?> GetByIdAsync(object id, CancellationToken cancellationToken)
    {
        return await _dbSet
            .FindAsync([id], cancellationToken)
            .ConfigureAwait(false);
    }

    void IGenericRepository<T>.Detach(T entity)
    {
        context.Entry(entity).State = EntityState.Detached;
    }
}