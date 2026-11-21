using Dotnetstore.JournalSystem.Api.SharedKernel.Security;
using Dotnetstore.Security.Handlers;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal static class AddressEncryption
{
    internal static Address Encrypt(this Address address, IEncryptionHandler handler)
    {
        ArgumentNullException.ThrowIfNull(address);

        return Copy(address, (value, field) => handler.EncryptField(value, address.Id, field));
    }

    internal static Address Decrypt(this Address address, IEncryptionHandler handler)
    {
        ArgumentNullException.ThrowIfNull(address);

        return Copy(address, (value, field) => handler.DecryptField(value, address.Id, field));
    }

    private static Address Copy(Address address, Func<string?, string, string?> transform)
    {
        return new Address(
            address.Id,
            address.AddressTypeId,
            address.ObjectId,
            transform(address.CO, nameof(Address.CO)),
            transform(address.Street1, nameof(Address.Street1))!,
            transform(address.Street2, nameof(Address.Street2)),
            transform(address.PostalCode, nameof(Address.PostalCode))!,
            transform(address.City, nameof(Address.City))!,
            transform(address.State, nameof(Address.State))!,
            transform(address.Country, nameof(Address.Country))!,
            address.CreatedDate,
            address.CreatedBy,
            address.LastUpdatedDate,
            address.LastUpdatedBy,
            address.DeletedDate,
            address.DeletedBy,
            address.IsDeleted,
            address.IsSystem,
            address.IsGdpr);
    }
}
