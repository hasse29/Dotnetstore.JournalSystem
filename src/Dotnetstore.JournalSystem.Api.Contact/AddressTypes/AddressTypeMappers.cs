using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.AddressTypes;

internal static class AddressTypeMappers
{
    internal static AddressTypeResponse ToAddressTypeResponse(this AddressType addressType)
    {
        return new AddressTypeResponse(
            addressType.Id,
            addressType.Name,
            addressType.Description);
    }
}
