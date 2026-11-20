using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetstore.JournalSystem.Integration.Tests;

internal static class TestHost
{
    internal const string OpenApiUrl = "/openapi/v1.json";
    internal const string EncryptionKeyId = "test";

    // A fixed key so tests are repeatable; it protects nothing real.
    internal const string EncryptionKey = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=";

    internal const string TokenSigningKeyId = "k1";

    // A fixed key so tests are repeatable; it protects nothing real.
    internal const string TokenSigningKey = "ZGV2LW9ubHktdGVzdC1zaWduaW5nLWtleS0zMi1ieXRlcyE=";

    [SuppressMessage("Reliability", "CA2000", Justification = "The derived factory returned by WithWebHostBuilder is disposed by the caller.")]
    internal static WebApplicationFactory<Host.Program> Create(
        string environment,
        IReadOnlyDictionary<string, string?>? settings = null,
        Action<IServiceCollection>? configureServices = null) =>
        new WebApplicationFactory<Host.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.UseSetting("Encryption:Keys:" + EncryptionKeyId, EncryptionKey);
            builder.UseSetting("Encryption:CurrentKeyId", EncryptionKeyId);
            builder.UseSetting("Tokens:SigningKeys:" + TokenSigningKeyId, TokenSigningKey);

            foreach (var setting in settings ?? new Dictionary<string, string?>())
            {
                builder.UseSetting(setting.Key, setting.Value);
            }

            if (configureServices is not null)
            {
                builder.ConfigureServices(configureServices);
            }
        });
}
