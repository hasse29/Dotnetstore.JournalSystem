using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Contact.Data;

internal sealed class ContactDataContext(DbContextOptions<ContactDataContext> options) : DbContext(options)
{
    public DbSet<Address> Addresses { get; set; }
    public DbSet<AddressType> AddressTypes { get; set; }
    public DbSet<EmailAddressType> EmailAddressTypes { get; set; }
    public DbSet<EmailAddress> EmailAddresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContactDataContext).Assembly);
    }
}
