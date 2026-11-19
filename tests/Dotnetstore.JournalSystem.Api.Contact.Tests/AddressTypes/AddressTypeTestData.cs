using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.AddressTypes;

internal static class AddressTypeTestData
{
    internal static readonly DateTime CreatedDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    internal static AddressType Create(Guid id, string name, string description = "Description")
    {
        return new AddressType(
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
