using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests;

public sealed class CorsTests
{
    private const string OpenApiUrl = "/openapi/v1.json";
    private const string AllowedOrigin = "http://localhost:3000";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000", Justification = "The derived factory returned by WithWebHostBuilder is disposed by the caller.")]
    private static WebApplicationFactory<Program> CreateFactory(string environment, params string[] origins) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                var settings = origins
                    .Select((origin, index) => KeyValuePair.Create($"Cors:AllowedOrigins:{index}", (string?)origin))
                    .ToList();
                config.AddInMemoryCollection(settings);
            });
        });

    private static HttpRequestMessage CreateRequest(HttpMethod method, string origin)
    {
        var request = new HttpRequestMessage(method, OpenApiUrl);
        request.Headers.Add("Origin", origin);
        return request;
    }

    [Fact]
    public async Task RequestFromAllowedOriginReturnsAllowOriginHeader()
    {
        // Arrange
        await using var factory = CreateFactory("Development", AllowedOrigin);
        using var client = factory.CreateClient();
        using var request = CreateRequest(HttpMethod.Get, AllowedOrigin);

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values).ShouldBeTrue();
        values.ShouldNotBeNull().ShouldHaveSingleItem().ShouldBe(AllowedOrigin);
    }

    [Fact]
    public async Task RequestFromDisallowedOriginReturnsNoAllowOriginHeader()
    {
        // Arrange
        await using var factory = CreateFactory("Development", AllowedOrigin);
        using var client = factory.CreateClient();
        using var request = CreateRequest(HttpMethod.Get, "http://evil.example");

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.Headers.Contains("Access-Control-Allow-Origin").ShouldBeFalse();
    }

    [Fact]
    public async Task RequestWhenNoOriginsConfiguredReturnsNoAllowOriginHeader()
    {
        // Arrange
        await using var factory = CreateFactory("Production");
        using var client = factory.CreateClient();
        using var request = CreateRequest(HttpMethod.Get, AllowedOrigin);

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.Headers.Contains("Access-Control-Allow-Origin").ShouldBeFalse();
    }

    [Fact]
    public async Task PreflightFromAllowedOriginReturnsAllowedMethodsAndHeaders()
    {
        // Arrange
        await using var factory = CreateFactory("Development", AllowedOrigin);
        using var client = factory.CreateClient();
        using var request = CreateRequest(HttpMethod.Options, AllowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", "content-type");

        // Act
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        response.Headers.GetValues("Access-Control-Allow-Origin").ShouldHaveSingleItem().ShouldBe(AllowedOrigin);
        string.Join(",", response.Headers.GetValues("Access-Control-Allow-Methods")).ShouldContain("POST");
        string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers")).ShouldContain("content-type", Case.Insensitive);
    }
}
