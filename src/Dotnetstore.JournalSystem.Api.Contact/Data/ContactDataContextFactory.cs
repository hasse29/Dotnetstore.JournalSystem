using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dotnetstore.JournalSystem.Api.Contact.Data;

/// <summary>Used only by <c>dotnet ef</c>. Generating a migration never connects, so the server is a placeholder.</summary>
internal sealed class ContactDataContextFactory : IDesignTimeDbContextFactory<ContactDataContext>
{
    public ContactDataContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer("Server=.;Database=journalsystem-design;Integrated Security=true;TrustServerCertificate=true", ContactDataContext.ConfigureSqlServer)
            .Options;

        return new ContactDataContext(options);
    }
}
