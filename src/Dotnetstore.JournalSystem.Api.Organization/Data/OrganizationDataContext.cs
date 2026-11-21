using Dotnetstore.JournalSystem.Api.Organization.AccessRequests;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Dotnetstore.JournalSystem.Api.Organization.Data;

internal sealed class OrganizationDataContext(DbContextOptions<OrganizationDataContext> options) : DbContext(options)
{
    internal const string MigrationsHistoryTable = "__EFMigrationsHistory";

    internal static void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sql) =>
        sql.MigrationsHistoryTable(MigrationsHistoryTable, DataSchemeConstants.SchemeName);

    public DbSet<AccessRequest> AccessRequests { get; set; }
    public DbSet<OwnCompany> OwnCompanies { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<WorkGroup> WorkGroups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationDataContext).Assembly);
    }
}
