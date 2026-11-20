namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;

public sealed class EntraSignInSettings
{
    internal const string SchemeName = "Entra";
    internal const string EnabledKey = "Authentication:Entra:Enabled";
    internal const string TenantIdKey = "AzureAd:TenantId";
    internal const string ClientIdKey = "AzureAd:ClientId";
    internal const string ClientSecretKey = "AzureAd:ClientCredentials:0:ClientSecret";
    internal const string AuthorityKey = "AzureAd:Authority";
    internal const string ApiScopeKey = "AzureAd:ApiScope";

    internal bool Enabled { get; private init; }

    internal Uri? Authority { get; private init; }

    internal string ClientId { get; private init; } = string.Empty;

    internal string ClientSecret { get; private init; } = string.Empty;

    /// <summary>The scope of the API the access token is for. Requesting it makes Entra issue a token the API accepts.</summary>
    internal string ApiScope { get; private init; } = string.Empty;

    internal static EntraSignInSettings Disabled { get; } = new();

    internal static EntraSignInSettings FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        if (!configuration.GetValue<bool>(EnabledKey))
        {
            return Disabled;
        }

        var tenantId = configuration[TenantIdKey];
        var clientId = configuration[ClientIdKey];
        var clientSecret = configuration[ClientSecretKey];
        var authorityOverride = configuration[AuthorityKey];

        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(clientId))
        {
            missing.Add(ClientIdKey);
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            missing.Add(ClientSecretKey);
        }

        if (string.IsNullOrWhiteSpace(tenantId) && string.IsNullOrWhiteSpace(authorityOverride))
        {
            missing.Add(TenantIdKey);
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Microsoft sign-in is enabled ({EnabledKey}) but these settings are missing: {string.Join(", ", missing)}.");
        }

        var authority = string.IsNullOrWhiteSpace(authorityOverride)
            ? new Uri($"https://login.microsoftonline.com/{tenantId}/v2.0")
            : new Uri(authorityOverride);

        return new EntraSignInSettings
        {
            Enabled = true,
            Authority = authority,
            ClientId = clientId!,
            ClientSecret = clientSecret!,
            ApiScope = configuration[ApiScopeKey] is { Length: > 0 } scope ? scope : $"api://{clientId}/access_as_user"
        };
    }
}
