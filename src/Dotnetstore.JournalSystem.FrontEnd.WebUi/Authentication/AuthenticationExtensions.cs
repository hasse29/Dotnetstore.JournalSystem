using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;

internal static class AuthenticationExtensions
{
    internal const string FailedSignInRedirect = "/Account/Login?error=external";

    internal static IServiceCollection AddJournalWebAuthentication(
        this IServiceCollection services,
        EntraSignInSettings entra)
    {
        services.AddSingleton(entra);

        var authentication = services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
                options.Cookie.Name = "journalsystem.session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.SlidingExpiration = false;
            });

        if (entra.Enabled)
        {
            authentication.AddOpenIdConnect(EntraSignInSettings.SchemeName, options => ConfigureEntra(options, entra));
        }

        services
            .AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        return services;
    }

    private static void ConfigureEntra(OpenIdConnectOptions options, EntraSignInSettings entra)
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Authority = entra.Authority!.ToString();
        options.ClientId = entra.ClientId;
        options.ClientSecret = entra.ClientSecret;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.ResponseMode = OpenIdConnectResponseMode.Query;
        options.UsePkce = true;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = false;
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = entra.Authority.Scheme == Uri.UriSchemeHttps;
        options.CallbackPath = "/signin-oidc";

        options.Scope.Clear();
        options.Scope.Add(OpenIdConnectScope.OpenId);
        options.Scope.Add("profile");
        options.Scope.Add(entra.ApiScope);

        options.TokenValidationParameters.NameClaimType = "preferred_username";

        // The correlation and nonce cookies default to SameSite=None, which browsers reject without Secure.
        // The response comes back as a top-level GET, so Lax is enough.
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.NonceCookie.SameSite = SameSiteMode.Lax;

        options.Events = new OpenIdConnectEvents
        {
            OnTicketReceived = context =>
            {
                var properties = context.Properties!;
                properties.IsPersistent = false;
                properties.AllowRefresh = false;

                // The session ends when the access token does, because the API rejects it after that.
                if (DateTimeOffset.TryParse(
                        properties.GetTokenValue("expires_at"),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out var expiresAt))
                {
                    properties.ExpiresUtc = expiresAt;
                }

                return Task.CompletedTask;
            },
            OnRemoteFailure = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication.Entra");
                EntraLog.SignInFailed(logger, context.Failure);

                var code = EntraFailureCode.Extract(
                    context.Failure,
                    context.Request.Query["error"],
                    context.Request.Query["error_description"]);
                context.Response.Redirect(code is null ? FailedSignInRedirect : $"{FailedSignInRedirect}&code={code}");
                context.HandleResponse();
                return Task.CompletedTask;
            }
        };
    }
}
