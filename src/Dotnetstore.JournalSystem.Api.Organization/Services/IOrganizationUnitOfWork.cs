using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

namespace Dotnetstore.JournalSystem.Api.Organization.Services;

internal interface IOrganizationUnitOfWork
{
    IOwnCompanyRepository OwnCompanies { get; }
    void CreateTransaction();
    ValueTask CommitAsync(CancellationToken cancellationToken = default);
    ValueTask RollbackAsync(CancellationToken cancellationToken = default);
    ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
