using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.JournalSystem.Api.Organization.WorkGroups;

internal sealed class WorkGroupConfiguration : IEntityTypeConfiguration<WorkGroup>
{
    public void Configure(EntityTypeBuilder<WorkGroup> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable(nameof(WorkGroup), DataSchemeConstants.SchemeName)
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.WorkGroupNameMaxLength);

        builder
            .Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.WorkGroupDescriptionMaxLength);
    }
}
