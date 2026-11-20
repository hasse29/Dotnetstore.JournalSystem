using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.AspNetCore.WebUtilities;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;

/// <summary>
/// Reads claims from a token the API has just issued over our own connection. The token is not validated here:
/// the API validates it on every call, and the cookie only keeps it so the UI can show who is signed in.
/// </summary>
internal static class JwtClaimsReader
{
    internal static ClaimsPrincipal CreatePrincipal(string accessToken, string username, string authenticationType)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, username) };

        foreach (var claim in ReadPayloadClaims(accessToken))
        {
            claims.Add(claim);
        }

        var identity = new ClaimsIdentity(claims, authenticationType, ClaimTypes.Name, AuthClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }

    private static IEnumerable<Claim> ReadPayloadClaims(string accessToken)
    {
        var parts = accessToken.Split('.');
        if (parts.Length != 3)
        {
            yield break;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(parts[1])));
        }
        catch (FormatException)
        {
            yield break;
        }
        catch (JsonException)
        {
            yield break;
        }

        using (document)
        {
            foreach (var type in new[] { AuthClaimTypes.UserId, AuthClaimTypes.OwnCompanyId })
            {
                if (document.RootElement.TryGetProperty(type, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    yield return new Claim(type, value.GetString()!);
                }
            }

            if (document.RootElement.TryGetProperty(AuthClaimTypes.Role, out var roles))
            {
                foreach (var role in EnumerateStrings(roles))
                {
                    yield return new Claim(AuthClaimTypes.Role, role);
                }
            }
        }
    }

    private static IEnumerable<string> EnumerateStrings(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                yield return element.GetString()!;
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray().Where(static x => x.ValueKind == JsonValueKind.String))
                {
                    yield return item.GetString()!;
                }

                break;
        }
    }
}
