using Dotnetstore.Security.Handlers;

namespace Dotnetstore.JournalSystem.Api.SharedKernel.Security;

public static class FieldEncryption
{
    // "dse1." + longest key id (32) + "." around a base64url payload of nonce (12) + tag (16) + ciphertext.
    private const int FormatOverhead = 5 + 32 + 1;
    private const int NonceAndTagBytes = 28;
    private const int MaxUtf8BytesPerChar = 3;

    public static string? EncryptField(this IEncryptionHandler handler, string? value, Guid recordId, string field)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentException.ThrowIfNullOrWhiteSpace(field);

        return value is null ? null : handler.Encrypt(value, Context(recordId, field));
    }

    public static string? DecryptField(this IEncryptionHandler handler, string? value, Guid recordId, string field)
    {
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentException.ThrowIfNullOrWhiteSpace(field);

        return value is null ? null : handler.Decrypt(value, Context(recordId, field));
    }

    public static int EncryptedMaxLength(int plaintextMaxLength)
    {
        var bytes = (plaintextMaxLength * MaxUtf8BytesPerChar) + NonceAndTagBytes;
        return FormatOverhead + ((bytes * 4) + 2) / 3;
    }

    // Binding each value to its record and column stops ciphertext being moved between rows or fields.
    private static string Context(Guid recordId, string field)
    {
        return $"{recordId:N}:{field}";
    }
}
