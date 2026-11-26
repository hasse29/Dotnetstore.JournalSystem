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
}
