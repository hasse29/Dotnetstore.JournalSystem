using Dotnetstore.JournalSystem.Api.Contact.Security;
using Dotnetstore.Security.Handlers;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal static class PhoneNumberEncryption
{
    internal static PhoneNumber Encrypt(this PhoneNumber phoneNumber, IEncryptionHandler handler)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);

        return Copy(phoneNumber, handler.EncryptField(phoneNumber.Number, phoneNumber.Id, nameof(PhoneNumber.Number))!);
    }

    internal static PhoneNumber Decrypt(this PhoneNumber phoneNumber, IEncryptionHandler handler)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);

        return Copy(phoneNumber, handler.DecryptField(phoneNumber.Number, phoneNumber.Id, nameof(PhoneNumber.Number))!);
    }

    private static PhoneNumber Copy(PhoneNumber phoneNumber, string number)
    {
        return new PhoneNumber(
            phoneNumber.Id,
            phoneNumber.PhoneNumberTypeId,
            phoneNumber.ObjectId,
            number,
            phoneNumber.CreatedDate,
            phoneNumber.CreatedBy,
            phoneNumber.LastUpdatedDate,
            phoneNumber.LastUpdatedBy,
            phoneNumber.DeletedDate,
            phoneNumber.DeletedBy,
            phoneNumber.IsDeleted,
            phoneNumber.IsSystem,
            phoneNumber.IsGdpr);
    }
}
