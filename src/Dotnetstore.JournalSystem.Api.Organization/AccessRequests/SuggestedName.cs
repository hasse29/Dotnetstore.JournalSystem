using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Api.Organization.AccessRequests;

/// <summary>Splits a Microsoft display name into first and last name to prefill the approval form.</summary>
internal static class SuggestedName
{
    internal static (string FirstName, string LastName) Split(string displayName, string fallback)
    {
        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            parts = [fallback];
        }

        var last = parts.Length > 1 ? parts[^1] : parts[0];
        var first = parts.Length > 1 ? string.Join(' ', parts[..^1]) : parts[0];

        return (Truncate(first), Truncate(last));
    }

    private static string Truncate(string value) =>
        value.Length <= DataSchemeConstants.UserNameMaxLength ? value : value[..DataSchemeConstants.UserNameMaxLength];
}
