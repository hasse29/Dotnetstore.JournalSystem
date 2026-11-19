using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal interface IRoleService
{
    Task<IEnumerable<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<Result<RoleResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<Result<RoleResponse>> CreateAsync(EditRoleRequest role, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result<RoleResponse>> UpdateAsync(Guid id, EditRoleRequest role, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
