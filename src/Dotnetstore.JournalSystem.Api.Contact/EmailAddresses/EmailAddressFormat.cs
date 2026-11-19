using System.Net.Mail;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal static class EmailAddressFormat
{
    internal static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !MailAddress.TryCreate(value, out var parsed))
        {
            return false;
        }

        // TryCreate also accepts display names such as "Name <a@b.c>" and dotless hosts; neither is a plain address.
        return string.Equals(parsed.Address, value, StringComparison.Ordinal) && parsed.Host.Contains('.', StringComparison.Ordinal);
    }
}
