using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.WorkGroups;

internal sealed class WorkGroupService(
    IOrganizationUnitOfWork organizationUnitOfWork) : IWorkGroupService
{
    async Task<IEnumerable<WorkGroupResponse>> IWorkGroupService.GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await organizationUnitOfWork
            .WorkGroups
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.ToWorkGroupResponse());
    }

    async ValueTask<Result<WorkGroupResponse>> IWorkGroupService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var workGroup = await organizationUnitOfWork
            .WorkGroups
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return workGroup is not null ?
            Result<WorkGroupResponse>.Success(workGroup.ToWorkGroupResponse()) :
            Result<WorkGroupResponse>.NotFound();
    }

    async ValueTask<Result<WorkGroupResponse>> IWorkGroupService.CreateAsync(EditWorkGroupRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        var workGroup = WorkGroupBuilder
            .Create()
            .WithCreateValues(request, userId)
            .Build();

        await organizationUnitOfWork
            .WorkGroups
            .AddAsync(workGroup, cancellationToken)
            .ConfigureAwait(false);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<WorkGroupResponse>.Success(workGroup.ToWorkGroupResponse()) :
            Result<WorkGroupResponse>.Error("Failed to create the work group.");
    }

    async ValueTask<Result<WorkGroupResponse>> IWorkGroupService.UpdateAsync(Guid id, EditWorkGroupRequest workGroup, Guid? userId, CancellationToken cancellationToken)
    {
        var existing = await organizationUnitOfWork
            .WorkGroups
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result<WorkGroupResponse>.NotFound();
        }

        var workGroupToUpdate = WorkGroupBuilder
            .Update()
            .WithUpdateValues(existing, workGroup, userId)
            .Build();

        organizationUnitOfWork
            .WorkGroups
            .Detach(existing);

        organizationUnitOfWork
            .WorkGroups
            .Update(workGroupToUpdate);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<WorkGroupResponse>.Success(workGroupToUpdate.ToWorkGroupResponse()) :
            Result<WorkGroupResponse>.Error("Failed to update the work group.");
    }

    async ValueTask<Result> IWorkGroupService.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await organizationUnitOfWork
            .WorkGroups
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.NotFound();
        }

        await organizationUnitOfWork
            .WorkGroups
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result.Success() :
            Result.Error("Failed to delete the work group.");
    }
}