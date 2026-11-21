using Dotnetstore.JournalSystem.Api.Organization.AccessRequests;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;

namespace Dotnetstore.JournalSystem.Api.Organization.Services;

internal interface IOrganizationUnitOfWork
{
    IAccessRequestRepository AccessRequests { get; }
    IOwnCompanyRepository OwnCompanies { get; }
    IRoleRepository Roles { get; }
    IUserRepository Users { get; }
    IWorkGroupRepository WorkGroups { get; }
    void CreateTransaction();
    ValueTask CommitAsync(CancellationToken cancellationToken = default);
    ValueTask RollbackAsync(CancellationToken cancellationToken = default);
    ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
