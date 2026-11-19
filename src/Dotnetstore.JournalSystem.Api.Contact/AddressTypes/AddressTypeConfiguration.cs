using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.JournalSystem.Api.Contact.AddressTypes;

internal sealed class AddressTypeConfiguration : IEntityTypeConfiguration<AddressType>
{
    public void Configure(EntityTypeBuilder<AddressType> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable(nameof(AddressType), DataSchemeConstants.ContactSchemeName)
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.AddressTypeNameMaxLength);

        builder
            .Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.AddressTypeDescriptionMaxLength);
    }
}
