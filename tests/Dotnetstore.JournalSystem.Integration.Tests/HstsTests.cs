using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests;

public sealed class HstsTests
{
    private static readonly Uri HttpsBaseAddress = new("https://localhost");

    [Fact]
    public async Task ProductionResponseOverHttpsContainsStrictTransportSecurityHeader()
    {
        // Arrange
        await using var factory = TestHost.Create("Production");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = HttpsBaseAddress });

        // Act
        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        var value = response.Headers.GetValues("Strict-Transport-Security").ShouldHaveSingleItem();
        value.ShouldContain("max-age=31536000");
        value.ShouldContain("includeSubDomains");
    }

    [Fact]
    public async Task DevelopmentResponseDoesNotContainStrictTransportSecurityHeader()
    {
        // Arrange
        await using var factory = TestHost.Create("Development");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = HttpsBaseAddress });

        // Act
        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.Headers.Contains("Strict-Transport-Security").ShouldBeFalse();
    }
}
