namespace Dotnetstore.JournalSystem.Shared.Responses.Contact;

public sealed record EmailAddressTypeResponse(
    Guid Id,
    string Name,
    string Description);
