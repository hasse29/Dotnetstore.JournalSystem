using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Roles;

namespace Dotnetstore.JournalSystem.Api.Organization.Services;

internal interface IOrganizationUnitOfWork
{
    IOwnCompanyRepository OwnCompanies { get; }
    IRoleRepository Roles { get; }
    void CreateTransaction();
    ValueTask CommitAsync(CancellationToken cancellationToken = default);
    ValueTask RollbackAsync(CancellationToken cancellationToken = default);
    ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
