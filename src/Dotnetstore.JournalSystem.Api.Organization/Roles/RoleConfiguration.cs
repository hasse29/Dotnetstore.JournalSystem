using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable(nameof(Role), DataSchemeConstants.SchemeName)
            .HasKey(x => x.Id);

        builder
            .Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.RoleNameMaxLength);

        builder
            .Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(DataSchemeConstants.RoleDescriptionMaxLength);
    }
}
