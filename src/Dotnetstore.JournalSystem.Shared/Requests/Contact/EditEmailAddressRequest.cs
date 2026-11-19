namespace Dotnetstore.JournalSystem.Shared.Requests.Contact;

public sealed record EditEmailAddressRequest(
    Guid EmailAddressTypeId,
    Guid ObjectId,
    string Address);
