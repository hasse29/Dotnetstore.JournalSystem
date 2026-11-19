namespace Dotnetstore.JournalSystem.Shared.Requests.Contact;

public sealed record EditPhoneNumberRequest(
    Guid PhoneNumberTypeId,
    Guid ObjectId,
    string Number);
