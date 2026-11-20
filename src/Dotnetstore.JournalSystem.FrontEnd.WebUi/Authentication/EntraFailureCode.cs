using System.Text.RegularExpressions;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;

/// <summary>
/// Pulls the short, non-sensitive error code out of a failed Microsoft sign-in (such as AADSTS50011 or access_denied)
/// so the login page can show what went wrong without exposing the full error text.
/// </summary>
internal static partial class EntraFailureCode
{
    [GeneratedRegex(@"AADSTS\d{3,7}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AadCode();

    [GeneratedRegex(@"^(AADSTS\d{3,7}|[a-z_]{1,40})$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Allowed();

    internal static string? Extract(Exception? failure, string? error, string? errorDescription)
    {
        foreach (var text in Texts(failure, errorDescription))
        {
            var match = AadCode().Match(text);
            if (match.Success)
            {
                return match.Value;
            }
        }

        return IsDisplayable(error) ? error : null;
    }

    internal static bool IsDisplayable(string? code) => !string.IsNullOrEmpty(code) && Allowed().IsMatch(code);

    private static IEnumerable<string> Texts(Exception? failure, string? errorDescription)
    {
        if (!string.IsNullOrEmpty(errorDescription))
        {
            yield return errorDescription;
        }

        for (var exception = failure; exception is not null; exception = exception.InnerException)
        {
            yield return exception.Message;
        }
    }
}
