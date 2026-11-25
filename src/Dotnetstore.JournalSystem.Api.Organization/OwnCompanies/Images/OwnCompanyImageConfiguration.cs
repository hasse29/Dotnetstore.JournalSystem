using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;

internal sealed class OwnCompanyImageConfiguration : IEntityTypeConfiguration<OwnCompanyImage>
{
    public void Configure(EntityTypeBuilder<OwnCompanyImage> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable(nameof(OwnCompanyImage), DataSchemeConstants.SchemeName)
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Id)
            .ValueGeneratedNever();

        builder
            .Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(OwnCompanyImageConstants.FileNameMaxLength);

        builder
            .Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(OwnCompanyImageConstants.ContentTypeMaxLength);

        builder
            .Property(x => x.Content)
            .IsRequired();

        builder
            .HasOne<OwnCompany>()
            .WithMany()
            .HasForeignKey(x => x.OwnCompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OwnCompanyId, "IX_OwnCompanyImage_OwnCompanyId");

        builder
            .HasIndex(x => x.OwnCompanyId, "IX_OwnCompanyImage_Logo")
            .IsUnique()
            .HasFilter("[IsLogo] = 1");
    }
}
