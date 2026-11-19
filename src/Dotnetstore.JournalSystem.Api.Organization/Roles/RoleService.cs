using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal sealed class RoleService(
    IOrganizationUnitOfWork organizationUnitOfWork) : IRoleService
{
    async Task<IEnumerable<RoleResponse>> IRoleService.GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await organizationUnitOfWork
            .Roles
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.ToRoleResponse());
    }

    async ValueTask<Result<RoleResponse>> IRoleService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await organizationUnitOfWork
            .Roles
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return role is not null ?
            Result<RoleResponse>.Success(role.ToRoleResponse()) :
            Result<RoleResponse>.NotFound();
    }

    async ValueTask<Result<RoleResponse>> IRoleService.CreateAsync(EditRoleRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        var role = RoleBuilder
            .Create()
            .WithCreateValues(request, userId)
            .Build();
        
        await organizationUnitOfWork
            .Roles
            .AddAsync(role, cancellationToken)
            .ConfigureAwait(false);
        
        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
        
        return result > 0 ?
            Result<RoleResponse>.Success(role.ToRoleResponse()) :
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
            .WithUpdateValues(existing, role, userId)
            .Build();
        
        organizationUnitOfWork
            .Roles
            .Detach(existing);

        organizationUnitOfWork
            .Roles
            .Update(roleToUpdate);
        
        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
        
        return result > 0 ?
            Result<RoleResponse>.Success(roleToUpdate.ToRoleResponse()) :
            Result<RoleResponse>.Error("Failed to update the role.");
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