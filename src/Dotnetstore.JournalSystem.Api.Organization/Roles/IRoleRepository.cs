using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal interface IRoleRepository : IGenericRepository<Role>
{
    /// <summary>Whether a role other than <paramref name="exceptId"/> has the name, as the database compares text.</summary>
    Task<bool> ExistsWithNameAsync(string name, Guid? exceptId = null, CancellationToken cancellationToken = default);

    /// <summary>The role's name and rights, untracked; null when there is no such role.</summary>
    Task<RoleAccess?> GetAccessAsync(Guid roleId, CancellationToken cancellationToken = default);

    Task<ILookup<Guid, string>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Makes the role hold exactly these rights once the changes are saved.</summary>
    Task SetPermissionsAsync(Guid roleId, IReadOnlyCollection<string> permissions, CancellationToken cancellationToken = default);
}
