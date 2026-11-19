using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;

internal static class EmailAddressTestData
{
    internal static readonly DateTime CreatedDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
    internal static readonly Guid EmailAddressTypeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    internal static readonly Guid ObjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    internal static EmailAddress Create(
        Guid id,
        string address = "anna@example.com",
        Guid? emailAddressTypeId = null,
        Guid? objectId = null)
    {
        return new EmailAddress(
            id,
            emailAddressTypeId ?? EmailAddressTypeId,
            objectId ?? ObjectId,
            address,
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
