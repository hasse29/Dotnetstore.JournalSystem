using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Dotnetstore.JournalSystem.Host.Extensions;

internal static class WebApplicationExtensions
{
    internal const string CorsPolicyName = "DefaultCors";

    internal static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        builder.Services
            .AddOpenApi()
            .AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            }))
            .AddRateLimiter(_ => { });

        builder.Services
            .AddOptions<RateLimiterOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                var permitLimit = configuration.GetValue("RateLimiting:PermitLimit", 100);
                var windowSeconds = configuration.GetValue("RateLimiting:WindowSeconds", 60);

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromSeconds(windowSeconds),
                            QueueLimit = 0
                        }));
                options.OnRejected = (context, _) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                    }

                    return ValueTask.CompletedTask;
                };
            });

        return builder;
    }

    internal static WebApplication RegisterPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app
                .MapOpenApi();
        }

        app
            .UseHttpsRedirection()
            .UseCors(CorsPolicyName)
            .UseRateLimiter();

        return app;
    }

    internal static async Task RunApplicationAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        await app
            .RunAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
