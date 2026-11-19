using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable(nameof(Address), DataSchemeConstants.ContactSchemeName)
            .HasKey(x => x.Id);

        builder
            .Property(x => x.CO)
            .HasMaxLength(DataSchemeConstants.AddressCOMaxLength);

        builder
            .Property(x => x.Street1)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.AddressStreetMaxLength);

        builder
            .Property(x => x.Street2)
            .HasMaxLength(DataSchemeConstants.AddressStreetMaxLength);

        builder
            .Property(x => x.PostalCode)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.AddressPostalCodeMaxLength);

        builder
            .Property(x => x.City)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.AddressCityMaxLength);

        builder
            .Property(x => x.State)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.AddressStateMaxLength);

        builder
            .Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.AddressCountryMaxLength);
    }
}
