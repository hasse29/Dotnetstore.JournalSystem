using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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
