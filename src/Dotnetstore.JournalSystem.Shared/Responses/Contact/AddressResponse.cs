namespace Dotnetstore.JournalSystem.Shared.Responses.Contact;

public sealed record AddressResponse(
    Guid Id,
    Guid AddressTypeId,
    Guid ObjectId,
    string? CO,
    string Street1,
    string? Street2,
    string PostalCode,
    string City,
    string State,
    string Country);
