using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal sealed class RoleRepository(OrganizationDataContext dataContext) : GenericRepository<Role>(dataContext), IRoleRepository
{
    async Task<bool> IRoleRepository.ExistsWithNameAsync(string name, Guid? exceptId, CancellationToken cancellationToken)
    {
        return await dataContext.Roles
            .AsNoTracking()
            .AnyAsync(x => x.Name == name && x.Id != exceptId, cancellationToken)
            .ConfigureAwait(false);
    }

    async Task<RoleAccess?> IRoleRepository.GetAccessAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var name = await dataContext.Roles
            .AsNoTracking()
            .Where(x => x.Id == roleId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
        if (name is null)
        {
            return null;
        }

        var permissions = await dataContext.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == roleId)
            .Select(x => x.Permission)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new RoleAccess(name, permissions);
    }

    async Task<ILookup<Guid, string>> IRoleRepository.GetAllPermissionsAsync(CancellationToken cancellationToken)
    {
        var all = await dataContext.RolePermissions
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return all.ToLookup(x => x.RoleId, x => x.Permission);
    }

    async Task IRoleRepository.SetPermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken)
    {
        var current = await dataContext.RolePermissions
            .Where(x => x.RoleId == roleId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        dataContext.RolePermissions.RemoveRange(current.Where(x => !permissions.Contains(x.Permission)));
        dataContext.RolePermissions.AddRange(permissions
            .Except(current.Select(x => x.Permission), StringComparer.Ordinal)
            .Select(x => new RolePermission(roleId, x)));
    }
}
