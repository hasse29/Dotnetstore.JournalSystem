using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Extensions;

public sealed class SecurityLoggingTests
{
    [Fact]
    public async Task RateLimitRejectionIsLoggedWithoutClientAddress()
    {
        // Arrange
        using var provider = new CapturingLoggerProvider();
        var settings = new Dictionary<string, string?> { ["RateLimiting:PermitLimit"] = "1" };
        await using var factory = TestHost.Create(
            "Production",
            settings,
            services => services.AddLogging(logging => logging.AddProvider(provider)));
        using var client = factory.CreateClient();
        using var allowed = await client.GetAsync(new Uri("/health", UriKind.Relative), TestContext.Current.CancellationToken);

        // Act
        using var rejected = await client.GetAsync(new Uri("/health", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        var entry = provider.Entries.Where(e => e.Message.Contains("Rate limit exceeded", StringComparison.Ordinal)).ToList().ShouldHaveSingleItem();
        entry.Level.ShouldBe(LogLevel.Warning);
        entry.Message.ShouldBe("Rate limit exceeded for GET /health");
    }

    [Fact]
    public async Task UnauthorizedResponseIsLogged()
    {
        // Arrange
        using var provider = new CapturingLoggerProvider();
        await using var factory = TestHost.Create(
            "Production",
            configureServices: services =>
            {
                services.AddLogging(logging => logging.AddProvider(provider));
                services.AddSingleton<IStartupFilter, UnauthorizedEndpointStartupFilter>();
            });
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("/secret", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Unauthorized);
        var entry = provider.Entries.Where(e => e.Message.Contains("Access denied", StringComparison.Ordinal)).ToList().ShouldHaveSingleItem();
        entry.Message.ShouldBe("Access denied with status 401 for GET /secret");
    }

    private sealed record LogEntry(LogLevel Level, string Message);

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        public ConcurrentBag<LogEntry> Entries { get; } = [];

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(Entries);

        public void Dispose()
        {
        }
    }

    private sealed class CapturingLogger(ConcurrentBag<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            entries.Add(new LogEntry(logLevel, formatter(state, exception)));
    }

    private sealed class UnauthorizedEndpointStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                next(app);
                app.Map("/secret", branch => branch.Run(context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }));
            };
    }
}
