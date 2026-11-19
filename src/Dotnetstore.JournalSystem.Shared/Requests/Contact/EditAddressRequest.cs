namespace Dotnetstore.JournalSystem.Shared.Requests.Contact;

public sealed record EditAddressRequest(
    Guid AddressTypeId,
    Guid ObjectId,
    string? CO,
    string Street1,
    string? Street2,
    string PostalCode,
    string City,
    string State,
    string Country);
