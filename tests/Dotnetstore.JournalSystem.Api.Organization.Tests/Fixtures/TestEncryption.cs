using Dotnetstore.Security.Extensions;
using Dotnetstore.Security.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;

internal static class TestEncryption
{
    internal const string KeyId = "test";

    // A fixed key so tests are repeatable; it protects nothing real.
    internal const string Key = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=";

    internal static IEncryptionHandler Create(string keyId = KeyId, string key = Key)
    {
        var services = new ServiceCollection();
        services.AddEncryption(options =>
        {
            options.Keys[keyId] = key;
            options.CurrentKeyId = keyId;
        });

        return services.BuildServiceProvider().GetRequiredService<IEncryptionHandler>();
    }
}
