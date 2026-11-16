using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.Data;

internal sealed class OrganizationDataContext(DbContextOptions<OrganizationDataContext> options) : DbContext(options)
{
    public DbSet<OwnCompany> OwnCompanies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrganizationDataContext).Assembly);
    }
}