using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Extensions;

public sealed class RateLimitingTests
{
    private const string OpenApiUrl = "/openapi/v1.json";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000", Justification = "The derived factory returned by WithWebHostBuilder is disposed by the caller.")]
    private static WebApplicationFactory<Program> CreateFactory(int permitLimit) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(
                [
                    KeyValuePair.Create("RateLimiting:PermitLimit", (string?)permitLimit.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                    KeyValuePair.Create("RateLimiting:WindowSeconds", (string?)"60")
                ]));
        });

    [Fact]
    public async Task RequestsWithinLimitSucceed()
    {
        // Arrange
        await using var factory = CreateFactory(permitLimit: 2);
        using var client = factory.CreateClient();

        // Act
        using var first = await client.GetAsync(new Uri(OpenApiUrl, UriKind.Relative), TestContext.Current.CancellationToken);
        using var second = await client.GetAsync(new Uri(OpenApiUrl, UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        second.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RequestExceedingLimitReturnsTooManyRequests()
    {
        // Arrange
        await using var factory = CreateFactory(permitLimit: 2);
        using var client = factory.CreateClient();

        // Act
        using var first = await client.GetAsync(new Uri(OpenApiUrl, UriKind.Relative), TestContext.Current.CancellationToken);
        using var second = await client.GetAsync(new Uri(OpenApiUrl, UriKind.Relative), TestContext.Current.CancellationToken);
        using var third = await client.GetAsync(new Uri(OpenApiUrl, UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        second.StatusCode.ShouldBe(HttpStatusCode.OK);
        third.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task RejectedRequestIncludesRetryAfterHeader()
    {
        // Arrange
        await using var factory = CreateFactory(permitLimit: 1);
        using var client = factory.CreateClient();
        using var allowed = await client.GetAsync(new Uri(OpenApiUrl, UriKind.Relative), TestContext.Current.CancellationToken);

        // Act
        using var rejected = await client.GetAsync(new Uri(OpenApiUrl, UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        rejected.Headers.RetryAfter.ShouldNotBeNull();
    }
}
