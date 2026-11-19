using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;

internal static class PhoneNumberTypeMappers
{
    internal static PhoneNumberTypeResponse ToPhoneNumberTypeResponse(this PhoneNumberType phoneNumberType)
    {
        return new PhoneNumberTypeResponse(
            phoneNumberType.Id,
            phoneNumberType.Name,
            phoneNumberType.Description);
    }
}
