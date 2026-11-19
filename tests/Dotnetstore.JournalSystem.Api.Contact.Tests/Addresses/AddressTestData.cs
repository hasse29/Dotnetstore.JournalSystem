using Dotnetstore.JournalSystem.Api.Contact.Addresses;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Addresses;

internal static class AddressTestData
{
    internal static readonly DateTime CreatedDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
    internal static readonly Guid AddressTypeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    internal static readonly Guid ObjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    internal static Address Create(
        Guid id,
        string street1 = "Main Street 1",
        string? street2 = null,
        string postalCode = "12345",
        string city = "Stockholm",
        string state = "Stockholm",
        string country = "Sweden",
        Guid? addressTypeId = null,
        Guid? objectId = null,
        string? co = null)
    {
        return new Address(
            id,
            addressTypeId ?? AddressTypeId,
            objectId ?? ObjectId,
            co,
            street1,
            street2,
            postalCode,
            city,
            state,
            country,
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
