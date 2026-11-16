using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Extensions;

public sealed class ForwardedHeadersTests
{
    private static readonly Dictionary<string, string?> OneRequestLimit = new()
    {
        ["RateLimiting:PermitLimit"] = "1",
        ["RateLimiting:WindowSeconds"] = "60"
    };

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2007", Justification = "Test helper; no synchronization context.")]
    private static async Task<(HttpStatusCode First, HttpStatusCode Second)> SendTwoRequestsAsync(
        IPAddress proxyAddress,
        IReadOnlyDictionary<string, string?> settings)
    {
        await using var factory = TestHost.Create(
            "Production",
            settings,
            services => services.AddSingleton<IStartupFilter>(new RemoteAddressStartupFilter(proxyAddress)));
        using var client = factory.CreateClient();

        using var first = new HttpRequestMessage(HttpMethod.Get, new Uri("/health", UriKind.Relative));
        first.Headers.Add("X-Forwarded-For", "203.0.113.10");
        using var second = new HttpRequestMessage(HttpMethod.Get, new Uri("/health", UriKind.Relative));
        second.Headers.Add("X-Forwarded-For", "203.0.113.11");

        using var firstResponse = await client.SendAsync(first, TestContext.Current.CancellationToken);
        using var secondResponse = await client.SendAsync(second, TestContext.Current.CancellationToken);

        return (firstResponse.StatusCode, secondResponse.StatusCode);
    }

    [Fact]
    public async Task ForwardedForFromTrustedProxyIsUsedAsClientAddress()
    {
        // Arrange
        var proxy = IPAddress.Loopback;

        // Act
        var (first, second) = await SendTwoRequestsAsync(proxy, OneRequestLimit);

        // Assert
        first.ShouldBe(HttpStatusCode.OK);
        second.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ForwardedForFromUntrustedProxyIsIgnored()
    {
        // Arrange
        var proxy = IPAddress.Parse("198.51.100.7");

        // Act
        var (first, second) = await SendTwoRequestsAsync(proxy, OneRequestLimit);

        // Assert
        first.ShouldBe(HttpStatusCode.OK);
        second.ShouldBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task ForwardedForFromConfiguredKnownProxyIsUsed()
    {
        // Arrange
        var proxy = IPAddress.Parse("198.51.100.7");
        var settings = new Dictionary<string, string?>(OneRequestLimit)
        {
            ["ForwardedHeaders:KnownProxies:0"] = proxy.ToString()
        };

        // Act
        var (first, second) = await SendTwoRequestsAsync(proxy, settings);

        // Assert
        first.ShouldBe(HttpStatusCode.OK);
        second.ShouldBe(HttpStatusCode.OK);
    }

    private sealed class RemoteAddressStartupFilter(IPAddress address) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                app.Use((context, nextMiddleware) =>
                {
                    context.Connection.RemoteIpAddress = address;
                    return nextMiddleware(context);
                });
                next(app);
            };
    }
}
