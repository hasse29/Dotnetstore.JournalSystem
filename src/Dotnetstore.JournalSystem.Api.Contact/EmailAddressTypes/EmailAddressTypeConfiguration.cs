using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;

internal sealed class EmailAddressTypeConfiguration : IEntityTypeConfiguration<EmailAddressType>
{
    public void Configure(EntityTypeBuilder<EmailAddressType> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable(nameof(EmailAddressType), DataSchemeConstants.ContactSchemeName)
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.EmailAddressTypeNameMaxLength);

        builder
            .Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.EmailAddressTypeDescriptionMaxLength);
    }
}
