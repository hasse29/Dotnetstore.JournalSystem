using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal static class PhoneNumberMappers
{
    internal static PhoneNumberResponse ToPhoneNumberResponse(this PhoneNumber phoneNumber)
    {
        return new PhoneNumberResponse(
            phoneNumber.Id,
            phoneNumber.PhoneNumberTypeId,
            phoneNumber.ObjectId,
            phoneNumber.Number);
    }
}
