using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.WorkGroups;

internal interface IWorkGroupService
{
    Task<IEnumerable<WorkGroupResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<Result<WorkGroupResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<Result<WorkGroupResponse>> CreateAsync(EditWorkGroupRequest workGroup, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result<WorkGroupResponse>> UpdateAsync(Guid id, EditWorkGroupRequest workGroup, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
