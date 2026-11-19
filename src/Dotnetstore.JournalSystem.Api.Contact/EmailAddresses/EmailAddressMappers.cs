using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal static class EmailAddressMappers
{
    internal static EmailAddressResponse ToEmailAddressResponse(this EmailAddress emailAddress)
    {
        return new EmailAddressResponse(
            emailAddress.Id,
            emailAddress.EmailAddressTypeId,
            emailAddress.ObjectId,
            emailAddress.Address);
    }
}
