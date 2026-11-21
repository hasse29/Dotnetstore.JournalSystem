using Dotnetstore.JournalSystem.Api.SharedKernel.Security;
using Dotnetstore.Security.Handlers;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal static class EmailAddressEncryption
{
    internal static EmailAddress Encrypt(this EmailAddress emailAddress, IEncryptionHandler handler)
    {
        ArgumentNullException.ThrowIfNull(emailAddress);

        return Copy(emailAddress, handler.EncryptField(emailAddress.Address, emailAddress.Id, nameof(EmailAddress.Address))!);
    }

    internal static EmailAddress Decrypt(this EmailAddress emailAddress, IEncryptionHandler handler)
    {
        ArgumentNullException.ThrowIfNull(emailAddress);

        return Copy(emailAddress, handler.DecryptField(emailAddress.Address, emailAddress.Id, nameof(EmailAddress.Address))!);
    }

    private static EmailAddress Copy(EmailAddress emailAddress, string address)
    {
        return new EmailAddress(
            emailAddress.Id,
            emailAddress.EmailAddressTypeId,
            emailAddress.ObjectId,
            address,
            emailAddress.CreatedDate,
            emailAddress.CreatedBy,
            emailAddress.LastUpdatedDate,
            emailAddress.LastUpdatedBy,
            emailAddress.DeletedDate,
            emailAddress.DeletedBy,
            emailAddress.IsDeleted,
            emailAddress.IsSystem,
            emailAddress.IsGdpr);
    }
}
