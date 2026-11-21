using Dotnetstore.JournalSystem.Api.SharedKernel.Security;
using Dotnetstore.Security.Handlers;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal sealed class PersonalIdentityNumberProtector(IEncryptionHandler encryption) : IPersonalIdentityNumberProtector
{
    private const string Field = nameof(User.SocialSecurityNumber);

    string? IPersonalIdentityNumberProtector.Protect(string? personalIdentityNumber, Guid userId)
    {
        var normalized = PersonalIdentityNumber.Normalize(personalIdentityNumber);
        return normalized is null ? null : encryption.EncryptField(normalized, userId, Field);
    }

    string? IPersonalIdentityNumberProtector.Mask(string? stored, Guid userId)
    {
        if (string.IsNullOrEmpty(stored))
        {
            return null;
        }

        var plain = encryption.DecryptField(stored, userId, Field);
        return plain is null ? null : PersonalIdentityNumber.Mask(plain);
    }
}
