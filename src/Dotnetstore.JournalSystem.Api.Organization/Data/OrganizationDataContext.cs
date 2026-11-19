using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.Data;

internal sealed class OrganizationDataContext(DbContextOptions<OrganizationDataContext> options) : DbContext(options)
{
    public DbSet<OwnCompany> OwnCompanies { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationDataContext).Assembly);
    }
}