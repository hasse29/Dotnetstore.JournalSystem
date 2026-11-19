using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddressTypes;

internal static class EmailAddressTypeTestData
{
    internal static readonly DateTime CreatedDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    internal static EmailAddressType Create(Guid id, string name, string description = "Description")
    {
        return new EmailAddressType(
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
