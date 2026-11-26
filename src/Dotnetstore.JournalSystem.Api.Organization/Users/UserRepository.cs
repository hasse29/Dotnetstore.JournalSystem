using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal sealed class UserRepository(OrganizationDataContext dataContext) : GenericRepository<User>(dataContext), IUserRepository
{
    async Task<User?> IUserRepository.GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await dataContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken)
            .ConfigureAwait(false);
    }

    async Task<User?> IUserRepository.GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        return await dataContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken)
            .ConfigureAwait(false);
    }

    async Task<User?> IUserRepository.FindUntrackedAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dataContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    async Task<bool> IUserRepository.ExistsInOwnCompanyAsync(Guid ownCompanyId, CancellationToken cancellationToken)
    {
        return await dataContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.OwnCompanyId == ownCompanyId, cancellationToken)
            .ConfigureAwait(false);
    }

    async Task<bool> IUserRepository.ExistsWithRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return await dataContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.RoleId == roleId, cancellationToken)
            .ConfigureAwait(false);
    }

    async Task IUserRepository.RegisterFailedLoginAsync(
        Guid id,
        int maxFailedAttempts,
        DateTime lockoutEnd,
        CancellationToken cancellationToken)
    {
        await dataContext.Users
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.IsLockedOut, x => x.AccessFailedCount + 1 >= maxFailedAttempts)
                    .SetProperty(x => x.LockoutEnd, x => x.AccessFailedCount + 1 >= maxFailedAttempts ? lockoutEnd : x.LockoutEnd)
                    .SetProperty(x => x.AccessFailedCount, x => x.AccessFailedCount + 1),
                cancellationToken)
            .ConfigureAwait(false);
    }

    async Task IUserRepository.RegisterSuccessfulLoginAsync(
        Guid id,
        DateTime loginAt,
        string? newPasswordHash,
        CancellationToken cancellationToken)
    {
        var query = dataContext.Users.Where(x => x.Id == id);

        if (newPasswordHash is null)
        {
            await query
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.AccessFailedCount, 0)
                        .SetProperty(x => x.IsLockedOut, false)
                        .SetProperty(x => x.LockoutEnd, (DateTime?)null)
                        .SetProperty(x => x.LastLoginAt, (DateTime?)loginAt),
                    cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        await query
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.AccessFailedCount, 0)
                    .SetProperty(x => x.IsLockedOut, false)
                    .SetProperty(x => x.LockoutEnd, (DateTime?)null)
                    .SetProperty(x => x.LastLoginAt, (DateTime?)loginAt)
                    .SetProperty(x => x.PasswordHash, newPasswordHash),
                cancellationToken)
            .ConfigureAwait(false);
    }

    async Task IUserRepository.ClearLockoutAsync(Guid id, CancellationToken cancellationToken)
    {
        await dataContext.Users
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.AccessFailedCount, 0)
                    .SetProperty(x => x.IsLockedOut, false)
                    .SetProperty(x => x.LockoutEnd, (DateTime?)null),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
