using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.Security.AspNetCore;
using Dotnetstore.Security.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Dotnetstore.JournalSystem.Host.Extensions;

internal static class AuthenticationExtensions
{
    internal const string TokensSection = HostApplicationBuilderExtensions.TokensConfigurationSection;
    internal const string EntraSection = "AzureAd";
    internal const string GraphSection = "MicrosoftGraph";

    extension(WebApplicationBuilder builder)
    {
        internal WebApplicationBuilder AddJournalAuthentication()
        {
            var configuration = builder.Configuration;
            var entraEnabled = configuration.GetValue<bool>(HostApplicationBuilderExtensions.EntraEnabledConfigurationKey);
            var localIssuer = configuration[$"{TokensSection}:Issuer"];

            var authentication = builder.Services
                .AddAuthentication(AuthSchemes.Journal)
                .AddPolicyScheme(AuthSchemes.Journal, displayName: null, options =>
                {
                    options.ForwardDefaultSelector = context => SelectScheme(context, localIssuer, entraEnabled);
                })
                .AddSecurityJwtBearer(AuthSchemes.Local);

            if (entraEnabled)
            {
                ValidateEntraConfiguration(configuration);

                authentication
                    .AddMicrosoftIdentityWebApi(configuration.GetSection(EntraSection), jwtBearerScheme: AuthSchemes.Entra)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddMicrosoftGraph(configuration.GetSection(GraphSection))
                    .AddInMemoryTokenCaches();

                // v2 access tokens carry the bare client id as audience; v1 tokens carry the api:// form.
                var clientId = configuration[$"{EntraSection}:ClientId"];
                builder.Services.PostConfigure<Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions>(
                    AuthSchemes.Entra,
                    options => options.TokenValidationParameters.ValidAudiences =
                        [clientId!, $"api://{clientId}", configuration[$"{EntraSection}:Audience"]!]);
            }

            builder.Services
                .AddAuthorizationBuilder()
                .AddPolicy(AuthPolicies.Authenticated, policy => policy.RequireAuthenticatedUser())
                .AddPolicy(AuthPolicies.Administrator, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(AuthClaimTypes.Role, RoleNames.Administrator));

            return builder;
        }
    }

    extension(WebApplication app)
    {
        internal WebApplication UseJournalAuthentication()
        {
            app
                .UseAuthentication()
                .UseAuthorization();

            return app;
        }
    }

    // Fails at startup with an actionable message instead of at the first Entra sign-in.
    internal static void ValidateEntraConfiguration(IConfiguration configuration)
    {
        var missing = new List<string>();

        foreach (var key in new[] { "TenantId", "ClientId" })
        {
            if (string.IsNullOrWhiteSpace(configuration[$"{EntraSection}:{key}"]))
            {
                missing.Add($"{EntraSection}:{key}");
            }
        }

        if (!configuration.GetSection($"{EntraSection}:ClientCredentials").GetChildren().Any())
        {
            missing.Add($"{EntraSection}:ClientCredentials (client secret or certificate; see docs/entra-setup.md)");
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Entra ID is enabled ({HostApplicationBuilderExtensions.EntraEnabledConfigurationKey}) but required settings are missing: {string.Join(", ", missing)}.");
        }
    }

    // Only the unvalidated issuer is read here, to route the token to the scheme that will fully validate it.
    private static string SelectScheme(HttpContext context, string? localIssuer, bool entraEnabled)
    {
        if (!entraEnabled)
        {
            return AuthSchemes.Local;
        }

        var header = context.Request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return AuthSchemes.Local;
        }

        var token = header[prefix.Length..].Trim();
        var handler = new JsonWebTokenHandler();
        if (!handler.CanReadToken(token))
        {
            return AuthSchemes.Local;
        }

        var issuer = handler.ReadJsonWebToken(token).Issuer;
        return string.Equals(issuer, localIssuer, StringComparison.Ordinal)
            ? AuthSchemes.Local
            : AuthSchemes.Entra;
    }
}
