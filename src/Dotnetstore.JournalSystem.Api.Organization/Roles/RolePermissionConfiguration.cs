using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable(nameof(RolePermission), DataSchemeConstants.SchemeName)
            .HasKey(x => new { x.RoleId, x.Permission });

        builder
            .Property(x => x.Permission)
            .HasMaxLength(DataSchemeConstants.PermissionMaxLength);

        builder
            .HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
