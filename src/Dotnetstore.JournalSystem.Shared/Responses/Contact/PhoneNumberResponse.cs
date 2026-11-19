namespace Dotnetstore.JournalSystem.Shared.Responses.Contact;

public sealed record PhoneNumberResponse(
    Guid Id,
    Guid PhoneNumberTypeId,
    Guid ObjectId,
    string Number);
