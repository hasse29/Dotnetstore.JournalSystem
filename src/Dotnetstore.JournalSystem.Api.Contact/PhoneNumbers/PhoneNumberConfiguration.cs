using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal sealed class PhoneNumberConfiguration : IEntityTypeConfiguration<PhoneNumber>
{
    public void Configure(EntityTypeBuilder<PhoneNumber> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable(nameof(PhoneNumber), DataSchemeConstants.ContactSchemeName)
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.PhoneNumberMaxLength);
    }
}
