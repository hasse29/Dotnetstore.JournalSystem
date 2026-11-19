namespace Dotnetstore.JournalSystem.Shared.Responses.Contact;

public sealed record EmailAddressResponse(
    Guid Id,
    Guid EmailAddressTypeId,
    Guid ObjectId,
    string Address);
