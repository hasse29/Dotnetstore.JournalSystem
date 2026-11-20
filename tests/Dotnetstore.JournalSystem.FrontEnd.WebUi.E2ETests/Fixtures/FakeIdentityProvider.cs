using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;

/// <summary>
/// A minimal OpenID Connect provider (authorization code flow with PKCE, RS256 id tokens) that stands in for Entra ID,
/// so the WebUi's redirect, callback and token handling can be driven by a real browser without real credentials.
/// The access token it issues is supplied by the test, because the API under test validates its own tokens.
/// </summary>
internal sealed class FakeIdentityProvider : IAsyncDisposable
{
    internal const string TenantId = "d603805b-1934-4b76-9f94-6587640f80bf";
    internal const string ClientId = "e2e-client";
    internal const string ClientSecret = "e2e-client-secret";
    internal const string UserName = "entra.user@example.com";
    internal const string DisplayName = "Entra User";
    internal const string ObjectId = "3f2b8c1e-7a4d-4e5b-9c63-1d0e5a7b9f42";

    private readonly ConcurrentDictionary<string, PendingCode> _codes = new();
    private readonly RSA _rsa = RSA.Create(2048);
    private readonly RsaSecurityKey _signingKey;
    private readonly Func<Task<string>> _accessTokenFactory;
    private WebApplication? _app;
    private string _baseAddress = string.Empty;

    private FakeIdentityProvider(Func<Task<string>> accessTokenFactory)
    {
        _accessTokenFactory = accessTokenFactory;
        _signingKey = new RsaSecurityKey(_rsa) { KeyId = "e2e-key" };
    }

    private string? TakeAccessToken()
    {
        var token = NextAccessToken;
        NextAccessToken = null;
        return token;
    }

    internal Uri Authority => new($"{_baseAddress}/{TenantId}/v2.0");

    /// <summary>When set, the next authorize call answers with this OAuth error instead of a code.</summary>
    internal string? NextAuthorizeError { get; set; }

    /// <summary>When set, the next token call returns this access token instead of a valid API token.</summary>
    internal string? NextAccessToken { get; set; }

    internal static async Task<FakeIdentityProvider> StartAsync(Func<Task<string>> accessTokenFactory)
    {
        var provider = new FakeIdentityProvider(accessTokenFactory);
        await provider.RunAsync();
        return provider;
    }

    public async ValueTask DisposeAsync()
    {
        _rsa.Dispose();

        if (_app is not null)
        {
            await _app.DisposeAsync();
        }
    }

    private async Task RunAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        var app = builder.Build();

        app.MapGet($"/{TenantId}/v2.0/.well-known/openid-configuration", () => Results.Json(new
        {
            issuer = Authority.ToString().TrimEnd('/'),
            authorization_endpoint = $"{_baseAddress}/{TenantId}/oauth2/v2.0/authorize",
            token_endpoint = $"{_baseAddress}/{TenantId}/oauth2/v2.0/token",
            jwks_uri = $"{_baseAddress}/{TenantId}/discovery/v2.0/keys",
            response_types_supported = new[] { "code" },
            subject_types_supported = new[] { "pairwise" },
            id_token_signing_alg_values_supported = new[] { "RS256" }
        }));

        app.MapGet($"/{TenantId}/discovery/v2.0/keys", () =>
        {
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(_signingKey);
            return Results.Json(new { keys = new[] { new { kty = jwk.Kty, use = "sig", kid = jwk.Kid, alg = "RS256", n = jwk.N, e = jwk.E } } });
        });

        app.MapGet($"/{TenantId}/oauth2/v2.0/authorize", Authorize);
        app.MapPost($"/{TenantId}/oauth2/v2.0/token", TokenAsync);

        await app.StartAsync();
        _app = app;
        _baseAddress = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!
            .Addresses.First().TrimEnd('/');
    }

    private IResult Authorize(HttpRequest request)
    {
        var redirectUri = request.Query["redirect_uri"].ToString();
        var state = request.Query["state"].ToString();

        if (NextAuthorizeError is { } error)
        {
            NextAuthorizeError = null;
            return Results.Redirect($"{redirectUri}?error={Uri.EscapeDataString(error)}&error_description=denied&state={Uri.EscapeDataString(state)}");
        }

        var code = Guid.NewGuid().ToString("N");
        _codes[code] = new PendingCode(
            request.Query["nonce"].ToString(),
            request.Query["code_challenge"].ToString(),
            redirectUri,
            request.Query["client_id"].ToString());

        return Results.Redirect($"{redirectUri}?code={code}&state={Uri.EscapeDataString(state)}");
    }

    private async Task<IResult> TokenAsync(HttpRequest request)
    {
        var form = await request.ReadFormAsync();

        if (form["client_secret"] != ClientSecret ||
            !_codes.TryRemove(form["code"].ToString(), out var pending) ||
            pending.RedirectUri != form["redirect_uri"].ToString() ||
            pending.ClientId != form["client_id"].ToString() ||
            ChallengeFor(form["code_verifier"].ToString()) != pending.CodeChallenge)
        {
            return Results.Json(new { error = "invalid_grant" }, statusCode: StatusCodes.Status400BadRequest);
        }

        var now = DateTime.UtcNow;
        var idToken = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Authority.ToString().TrimEnd('/'),
            Audience = ClientId,
            NotBefore = now,
            IssuedAt = now,
            Expires = now.AddHours(1),
            Claims = new Dictionary<string, object>
            {
                ["sub"] = "fake-subject",
                ["oid"] = ObjectId,
                ["name"] = DisplayName,
                ["preferred_username"] = UserName,
                ["nonce"] = pending.Nonce
            },
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256)
        });

        return Results.Json(new
        {
            token_type = "Bearer",
            expires_in = 3600,
            access_token = TakeAccessToken() ?? await _accessTokenFactory(),
            id_token = idToken
        });
    }

    private static string ChallengeFor(string verifier) =>
        Base64UrlEncoder.Encode(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));

    private sealed record PendingCode(string Nonce, string CodeChallenge, string RedirectUri, string ClientId);
}
