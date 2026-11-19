using System.Net;
using System.Text.Json;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Extensions;

public sealed class ScalarTests
{
    private const string ScalarUrl = "/scalar/v1";

    [Fact]
    public async Task ScalarReferenceIsServedInDevelopment()
    {
        // Arrange
        await using var factory = TestHost.Create("Development");
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri(ScalarUrl, UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/html");
    }

    [Fact]
    public async Task ScalarReferenceIsNotServedInProduction()
    {
        // Arrange
        await using var factory = TestHost.Create("Production");
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri(ScalarUrl, UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ScalarReferenceAllowsItsOwnScriptsThroughContentSecurityPolicy()
    {
        // Arrange
        await using var factory = TestHost.Create("Development");
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri(ScalarUrl, UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        var policy = response.Headers.GetValues("Content-Security-Policy").ShouldHaveSingleItem();
        policy.ShouldContain("script-src 'self'");
        policy.ShouldContain("frame-ancestors 'none'");
    }

    [Fact]
    public async Task OpenApiDocumentDescribesOwnCompanyEndpoints()
    {
        // Arrange
        await using var factory = TestHost.Create("Development");
        using var client = factory.CreateClient();

        // Act
        var body = await client.GetStringAsync(new Uri(TestHost.OpenApiUrl, UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        using var json = JsonDocument.Parse(body);
        var paths = json.RootElement.GetProperty("paths");
        paths.TryGetProperty("/api/organization/own-companies", out var collection).ShouldBeTrue();
        collection.TryGetProperty("get", out _).ShouldBeTrue();
        collection.TryGetProperty("post", out _).ShouldBeTrue();
        paths.TryGetProperty("/api/organization/own-companies/{id}", out var item).ShouldBeTrue();
        item.TryGetProperty("get", out _).ShouldBeTrue();
        item.TryGetProperty("put", out _).ShouldBeTrue();
        item.TryGetProperty("delete", out _).ShouldBeTrue();
    }
}
