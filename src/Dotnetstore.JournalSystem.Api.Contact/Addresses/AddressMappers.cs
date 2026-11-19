using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal static class AddressMappers
{
    internal static AddressResponse ToAddressResponse(this Address address)
    {
        return new AddressResponse(
            address.Id,
            address.AddressTypeId,
            address.ObjectId,
            address.CO,
            address.Street1,
            address.Street2,
            address.PostalCode,
            address.City,
            address.State,
            address.Country);
    }
}
