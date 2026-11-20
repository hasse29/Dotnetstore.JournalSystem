using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<Result<UserResponse>> CreateAsync(CreateUserRequest request, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result<UserResponse>> UpdateAsync(Guid id, EditUserRequest request, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
