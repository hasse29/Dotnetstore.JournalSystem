using Dotnetstore.BaseModel.Configurations;
using Dotnetstore.JournalSystem.Api.SharedKernel.Security;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal sealed class UserConfiguration : PersonIdentityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.Configure(builder);

        builder
            .ToTable(nameof(User), DataSchemeConstants.SchemeName)
            .HasKey(x => x.Id);

        builder
            .Property(x => x.SocialSecurityNumber)
            .HasMaxLength(FieldEncryption.EncryptedMaxLength(DataSchemeConstants.PersonalIdentityNumberMaxLength));

        builder
            .Property(x => x.AuthProvider)
            .HasConversion<int>();

        builder
            .Property(x => x.ExternalId)
            .HasMaxLength(DataSchemeConstants.UserExternalIdMaxLength);

        builder
            .HasIndex(x => x.ExternalId)
            .IsUnique()
            .HasFilter($"[{nameof(User.ExternalId)}] IS NOT NULL");

        builder
            .HasIndex(x => x.OwnCompanyId);

        builder
            .HasIndex(x => x.RoleId);
    }
}
