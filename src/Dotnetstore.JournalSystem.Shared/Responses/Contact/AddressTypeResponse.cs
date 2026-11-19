namespace Dotnetstore.JournalSystem.Shared.Responses.Contact;

public sealed record AddressTypeResponse(
    Guid Id,
    string Name,
    string Description);
