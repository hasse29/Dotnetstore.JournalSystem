using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal sealed class RoleService(
    IOrganizationUnitOfWork organizationUnitOfWork) : IRoleService
{
    private const string DuplicateNameMessage = "A role with that name already exists.";

    private async Task<IReadOnlyList<string>> GetPermissionsAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var access = await organizationUnitOfWork
            .Roles
            .GetAccessAsync(roleId, cancellationToken)
            .ConfigureAwait(false);

        return access?.Permissions ?? [];
    }

    private static EditRoleRequest Trimmed(EditRoleRequest request) => request with { Name = request.Name.Trim() };

    private async Task<bool> NameIsTakenAsync(string name, Guid? exceptId, CancellationToken cancellationToken) =>
        await organizationUnitOfWork
            .Roles
            .ExistsWithNameAsync(name, exceptId, cancellationToken)
            .ConfigureAwait(false);

    async Task<IEnumerable<RoleResponse>> IRoleService.GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await organizationUnitOfWork
            .Roles
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        var permissions = await organizationUnitOfWork
            .Roles
            .GetAllPermissionsAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.ToRoleResponse(permissions[x.Id]));
    }

    async ValueTask<Result<RoleResponse>> IRoleService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await organizationUnitOfWork
            .Roles
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (role is null)
        {
            return Result<RoleResponse>.NotFound();
        }

        var permissions = await GetPermissionsAsync(id, cancellationToken).ConfigureAwait(false);

        return Result<RoleResponse>.Success(role.ToRoleResponse(permissions));
    }

    async ValueTask<Result<RoleResponse>> IRoleService.CreateAsync(EditRoleRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        var role = RoleBuilder
            .Create()
            .WithCreateValues(Trimmed(request), userId)
            .Build();

        if (await NameIsTakenAsync(role.Name, null, cancellationToken).ConfigureAwait(false))
        {
            return Result<RoleResponse>.Conflict(DuplicateNameMessage);
        }

        await organizationUnitOfWork
            .Roles
            .AddAsync(role, cancellationToken)
            .ConfigureAwait(false);

        var permissions = Shared.Constants.Permissions.WithRequired(request.Permissions ?? []);
        await organizationUnitOfWork
            .Roles
            .SetPermissionsAsync(role.Id, permissions, cancellationToken)
            .ConfigureAwait(false);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<RoleResponse>.Success(role.ToRoleResponse(permissions)) :
            Result<RoleResponse>.Error("Failed to create the role.");
    }

    async ValueTask<Result<RoleResponse>> IRoleService.UpdateAsync(Guid id, EditRoleRequest role, Guid? userId, CancellationToken cancellationToken)
    {
        var existing = await organizationUnitOfWork
            .Roles
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result<RoleResponse>.NotFound();
        }

        var roleToUpdate = RoleBuilder
            .Update()
            .WithUpdateValues(existing, Trimmed(role), userId)
            .Build();

        // The administrator claim is the role's name, so a system role must keep it exactly.
        if (existing.IsSystem && !string.Equals(existing.Name, roleToUpdate.Name, StringComparison.Ordinal))
        {
            return Result<RoleResponse>.Conflict("System roles cannot be renamed.");
        }

        // Only a new name is checked, so a role that already shares its name can still be edited.
        if (!string.Equals(existing.Name, roleToUpdate.Name, StringComparison.OrdinalIgnoreCase)
            && await NameIsTakenAsync(roleToUpdate.Name, id, cancellationToken).ConfigureAwait(false))
        {
            return Result<RoleResponse>.Conflict(DuplicateNameMessage);
        }

        organizationUnitOfWork
            .Roles
            .Detach(existing);

        organizationUnitOfWork
            .Roles
            .Update(roleToUpdate);

        // A system role has every right, so none are stored for it.
        if (role.Permissions is { } requested && !roleToUpdate.IsSystem)
        {
            await organizationUnitOfWork
                .Roles
                .SetPermissionsAsync(id, Shared.Constants.Permissions.WithRequired(requested), cancellationToken)
                .ConfigureAwait(false);
        }

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        if (result <= 0)
        {
            return Result<RoleResponse>.Error("Failed to update the role.");
        }

        var permissions = await GetPermissionsAsync(id, cancellationToken).ConfigureAwait(false);

        return Result<RoleResponse>.Success(roleToUpdate.ToRoleResponse(permissions));
    }

    async ValueTask<Result> IRoleService.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await organizationUnitOfWork
            .Roles
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.NotFound();
        }

        if (existing.IsSystem)
        {
            return Result.Conflict("System roles cannot be deleted.");
        }

        var hasUsers = await organizationUnitOfWork
            .Users
            .ExistsWithRoleAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (hasUsers)
        {
            return Result.Conflict("The role is still used by users. Give them another role first.");
        }

        await organizationUnitOfWork
            .Roles
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result.Success() :
            Result.Error("Failed to delete the role.");
    }
}