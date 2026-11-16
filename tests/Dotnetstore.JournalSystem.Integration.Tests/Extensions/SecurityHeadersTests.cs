using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Extensions;

public sealed class SecurityHeadersTests
{
    [Fact]
    public async Task ResponseContainsSecurityHeaders()
    {
        // Arrange
        await using var factory = TestHost.Create("Development");
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri(TestHost.OpenApiUrl, UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.Headers.GetValues("X-Content-Type-Options").ShouldHaveSingleItem().ShouldBe("nosniff");
        response.Headers.GetValues("X-Frame-Options").ShouldHaveSingleItem().ShouldBe("DENY");
        response.Headers.GetValues("Referrer-Policy").ShouldHaveSingleItem().ShouldBe("no-referrer");
        response.Headers.GetValues("Content-Security-Policy").ShouldHaveSingleItem().ShouldContain("default-src 'none'");
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
    }

    [Fact]
    public async Task NotFoundResponseAlsoContainsSecurityHeaders()
    {
        // Arrange
        await using var factory = TestHost.Create("Production");
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("/does-not-exist", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
        response.Headers.Contains("X-Content-Type-Options").ShouldBeTrue();
    }
}
