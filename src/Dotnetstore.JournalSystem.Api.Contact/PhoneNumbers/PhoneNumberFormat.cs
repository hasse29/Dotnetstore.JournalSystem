namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal static class PhoneNumberFormat
{
    private const int MinDigits = 5;
    private const int MaxDigits = 15;

    internal static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        var digits = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsAsciiDigit(c))
            {
                digits++;
            }
            else if (c == '+' && i == 0)
            {
                continue;
            }
            else if (c is not (' ' or '-' or '(' or ')'))
            {
                return false;
            }
        }

        return digits is >= MinDigits and <= MaxDigits;
    }
}
