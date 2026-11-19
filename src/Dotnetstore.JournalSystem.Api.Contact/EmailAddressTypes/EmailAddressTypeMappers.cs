using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;

internal static class EmailAddressTypeMappers
{
    internal static EmailAddressTypeResponse ToEmailAddressTypeResponse(this EmailAddressType emailAddressType)
    {
        return new EmailAddressTypeResponse(
            emailAddressType.Id,
            emailAddressType.Name,
            emailAddressType.Description);
    }
}
