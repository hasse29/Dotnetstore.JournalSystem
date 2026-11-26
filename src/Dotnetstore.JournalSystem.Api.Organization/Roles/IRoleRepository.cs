using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal interface IRoleRepository : IGenericRepository<Role>
{
    /// <summary>Whether a role other than <paramref name="exceptId"/> has the name, as the database compares text.</summary>
    Task<bool> ExistsWithNameAsync(string name, Guid? exceptId = null, CancellationToken cancellationToken = default);
}
