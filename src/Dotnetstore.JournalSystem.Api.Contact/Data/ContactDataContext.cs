using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Dotnetstore.JournalSystem.Api.Contact.Data;

internal sealed class ContactDataContext(DbContextOptions<ContactDataContext> options) : DbContext(options)
{
    internal const string MigrationsHistoryTable = "__EFMigrationsHistory";

    internal static void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sql) =>
        sql.MigrationsHistoryTable(MigrationsHistoryTable, DataSchemeConstants.ContactSchemeName);

    public DbSet<Address> Addresses { get; set; }
    public DbSet<AddressType> AddressTypes { get; set; }
    public DbSet<PhoneNumberType> PhoneNumberTypes { get; set; }

    public DbSet<PhoneNumber> PhoneNumbers { get; set; }
    public DbSet<EmailAddressType> EmailAddressTypes { get; set; }
    public DbSet<EmailAddress> EmailAddresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContactDataContext).Assembly);
    }
}
