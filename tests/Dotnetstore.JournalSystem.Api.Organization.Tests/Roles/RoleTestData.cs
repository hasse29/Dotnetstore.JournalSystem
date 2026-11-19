using Dotnetstore.JournalSystem.Api.Organization.Roles;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;

internal static class RoleTestData
{
    internal static readonly DateTime CreatedDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    internal static Role Create(Guid id, string name, string description = "Description")
    {
        return new Role(
            id,
            name,
            description,
            CreatedDate,
            null,
            null,
            null,
            null,
            null,
            false,
            false,
            false);
    }
}
