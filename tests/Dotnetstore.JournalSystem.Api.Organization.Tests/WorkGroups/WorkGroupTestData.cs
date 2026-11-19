using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.WorkGroups;

internal static class WorkGroupTestData
{
    internal static readonly DateTime CreatedDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    internal static WorkGroup Create(Guid id, string name, string description = "Description")
    {
        return new WorkGroup(
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
