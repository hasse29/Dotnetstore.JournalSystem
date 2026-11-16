using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Dotnetstore.JournalSystem.Host.Extensions;

internal static class HealthEndpointsExtensions
{
    internal const string HealthPath = "/health";
    internal const string ReadyPath = "/ready";
    private const string LiveTag = "live";

    extension(WebApplication app)
    {
        internal WebApplication MapHealthEndpoints()
        {
            app
                .MapHealthChecks(HealthPath, new HealthCheckOptions
                {
                    Predicate = registration => registration.Tags.Contains(LiveTag)
                });

            if (app.Environment.IsDevelopment())
            {
                app
                    .MapHealthChecks(ReadyPath);
            }

            return app;
        }
    }
}
