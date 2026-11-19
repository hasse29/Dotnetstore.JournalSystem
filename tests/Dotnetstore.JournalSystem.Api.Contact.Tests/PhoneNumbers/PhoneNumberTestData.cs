using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;

internal static class PhoneNumberTestData
{
    internal static readonly DateTime CreatedDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
    internal static readonly Guid PhoneNumberTypeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    internal static readonly Guid ObjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    internal static PhoneNumber Create(
        Guid id,
        string number = "+46 70 123 45 67",
        Guid? phoneNumberTypeId = null,
        Guid? objectId = null)
    {
        return new PhoneNumber(
            id,
            phoneNumberTypeId ?? PhoneNumberTypeId,
            objectId ?? ObjectId,
            number,
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
