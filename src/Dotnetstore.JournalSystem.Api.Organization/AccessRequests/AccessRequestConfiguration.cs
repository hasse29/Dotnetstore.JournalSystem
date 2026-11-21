using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.JournalSystem.Api.Organization.AccessRequests;

internal sealed class AccessRequestConfiguration : IEntityTypeConfiguration<AccessRequest>
{
    public void Configure(EntityTypeBuilder<AccessRequest> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .ToTable(nameof(AccessRequest), DataSchemeConstants.SchemeName)
            .HasKey(x => x.Id);

        builder
            .Property(x => x.ExternalId)
            .HasMaxLength(DataSchemeConstants.UserExternalIdMaxLength)
            .IsRequired();

        builder
            .Property(x => x.DisplayName)
            .HasMaxLength(DataSchemeConstants.AccessRequestDisplayNameMaxLength)
            .IsRequired();

        builder
            .Property(x => x.Username)
            .HasMaxLength(DataSchemeConstants.UserUsernameMaxLength)
            .IsRequired();

        builder
            .Property(x => x.Status)
            .HasConversion<int>();

        builder
            .Property(x => x.DecisionNote)
            .HasMaxLength(DataSchemeConstants.AccessRequestDecisionNoteMaxLength);

        // One request per person: a rejected request is reopened by an administrator rather than duplicated.
        builder
            .HasIndex(x => x.ExternalId)
            .IsUnique();

        builder
            .HasIndex(x => x.Status);
    }
}
