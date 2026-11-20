using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;

/// <summary>
/// Describes who a token is for (audience, issuer, version, scopes, tenant) without including the token or any
/// personal data, so a rejected sign-in can be diagnosed from the log.
/// </summary>
internal static class JwtTokenSummary
{
    private static readonly string[] Claims = ["aud", "iss", "ver", "scp", "tid", "azp"];

    internal static string Describe(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return "no access token";
        }

        var parts = accessToken.Split('.');
        if (parts.Length != 3)
        {
            return "access token is not a JWT";
        }

        try
        {
            using var document = JsonDocument.Parse(Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(parts[1])));
            var found = Claims
                .Where(name => document.RootElement.TryGetProperty(name, out _))
                .Select(name => $"{name}={document.RootElement.GetProperty(name)}");
            return string.Join(", ", found);
        }
        catch (FormatException)
        {
            return "access token payload is unreadable";
        }
        catch (JsonException)
        {
            return "access token payload is unreadable";
        }
    }
}
