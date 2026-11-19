using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumberTypes;

internal static class PhoneNumberTypeTestData
{
    internal static readonly DateTime CreatedDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    internal static PhoneNumberType Create(Guid id, string name, string description = "Description")
    {
        return new PhoneNumberType(
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
