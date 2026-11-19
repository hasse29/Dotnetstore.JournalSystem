using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Contact.Data;

internal sealed class ContactDataContext(DbContextOptions<ContactDataContext> options) : DbContext(options)
{
    public DbSet<AddressType> AddressTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContactDataContext).Assembly);
    }
}
