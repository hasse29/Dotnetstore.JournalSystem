using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dotnetstore.JournalSystem.Api.Organization.Data;

/// <summary>Used only by <c>dotnet ef</c>. Generating a migration never connects, so the server is a placeholder.</summary>
internal sealed class OrganizationDataContextFactory : IDesignTimeDbContextFactory<OrganizationDataContext>
{
    public OrganizationDataContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlServer("Server=.;Database=journalsystem-design;Integrated Security=true;TrustServerCertificate=true", OrganizationDataContext.ConfigureSqlServer)
            .Options;

        return new OrganizationDataContext(options);
    }
}
