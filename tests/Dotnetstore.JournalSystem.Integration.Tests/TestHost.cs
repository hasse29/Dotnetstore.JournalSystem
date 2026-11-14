using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetstore.JournalSystem.Integration.Tests;

internal static class TestHost
{
    internal const string OpenApiUrl = "/openapi/v1.json";

    [SuppressMessage("Reliability", "CA2000", Justification = "The derived factory returned by WithWebHostBuilder is disposed by the caller.")]
    internal static WebApplicationFactory<Program> Create(
        string environment,
        IReadOnlyDictionary<string, string?>? settings = null,
        Action<IServiceCollection>? configureServices = null) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(settings ?? new Dictionary<string, string?>()));

            if (configureServices is not null)
            {
                builder.ConfigureServices(configureServices);
            }
        });
}
