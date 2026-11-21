using Dotnetstore.JournalSystem.Api.Organization.AccessRequests;
using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;

internal sealed class OrganizationTestDatabase : IDisposable
{
    private readonly DbContextOptions<OrganizationDataContext> _options;

    internal OrganizationTestDatabase(SqlServerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        Context = new OrganizationDataContext(_options);
        Context.Database.EnsureCreated();
        UnitOfWork = new OrganizationUnitOfWork(
            Context,
            new AccessRequestRepository(Context),
            new OwnCompanyRepository(Context),
            new RoleRepository(Context),
            new UserRepository(Context),
            new WorkGroupRepository(Context));
    }

    internal OrganizationDataContext Context { get; }

    internal OrganizationUnitOfWork UnitOfWork { get; }

    public void Dispose()
    {
        UnitOfWork.Dispose();
        Context.Dispose();
        using var cleanup = new OrganizationDataContext(_options);
        cleanup.Database.EnsureDeleted();
    }
}
