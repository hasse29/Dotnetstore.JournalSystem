using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);

    /// <summary>The user untracked, so that reading who is calling cannot disturb what the request then changes.</summary>
    Task<User?> FindUntrackedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsInOwnCompanyAsync(Guid ownCompanyId, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task RegisterFailedLoginAsync(
        Guid id,
        int maxFailedAttempts,
        DateTime lockoutEnd,
        CancellationToken cancellationToken = default);

    Task RegisterSuccessfulLoginAsync(
        Guid id,
        DateTime loginAt,
        string? newPasswordHash,
        CancellationToken cancellationToken = default);

    Task ClearLockoutAsync(Guid id, CancellationToken cancellationToken = default);
}
