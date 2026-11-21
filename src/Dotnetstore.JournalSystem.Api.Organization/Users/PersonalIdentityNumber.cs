namespace Dotnetstore.JournalSystem.Api.Organization.Users;

/// <summary>
/// Rules for a personal identity number (ten or twelve digits, hyphen or plus allowed). The number is stored as
/// digits only and is never shown in full.
/// </summary>
internal static class PersonalIdentityNumber
{
    private const string MaskedTail = "-****";

    /// <summary>Strips separators and spaces. Returns null for blank input.</summary>
    internal static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return new string(value.Where(c => c is not ('-' or '+' or ' ')).ToArray());
    }

    internal static bool IsValid(string? value)
    {
        var normalized = Normalize(value);
        return normalized is { Length: 10 or 12 } && normalized.All(char.IsAsciiDigit);
    }

    // Only the birth-date part is ever shown; the serial part and check digit stay hidden.
    internal static string Mask(string normalized)
    {
        ArgumentNullException.ThrowIfNull(normalized);

        return normalized switch
        {
            { Length: 12 } => normalized[..8] + MaskedTail,
            { Length: 10 } => normalized[..6] + MaskedTail,
            _ => "****"
        };
    }
}
