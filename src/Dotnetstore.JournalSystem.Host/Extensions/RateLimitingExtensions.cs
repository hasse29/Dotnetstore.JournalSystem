using System.Globalization;
using System.Threading.RateLimiting;
using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.Host.Logging;
using Microsoft.AspNetCore.RateLimiting;

namespace Dotnetstore.JournalSystem.Host.Extensions;

internal static class RateLimitingExtensions
{
    private const string UnknownClient = "unknown";

    extension(IServiceCollection services)
    {
        internal IServiceCollection AddRateLimiting()
        {
            services.AddRateLimiter(_ => { });

            services
                .AddOptions<RateLimiterOptions>()
                .Configure<IConfiguration, ILoggerFactory>(ConfigureLimiter);

            return services;
        }
    }

    private static void ConfigureLimiter(RateLimiterOptions options, IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        var permitLimit = configuration.GetValue("RateLimiting:PermitLimit", 100);
        var window = TimeSpan.FromSeconds(configuration.GetValue("RateLimiting:WindowSeconds", 60));
        var logger = loggerFactory.CreateLogger(SecurityLog.Category);

        var loginPermitLimit = configuration.GetValue("RateLimiting:LoginPermitLimit", 10);

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy(
            HostApplicationBuilderExtensions.LoginRateLimitPolicy,
            context => CreatePartition(context, loginPermitLimit, window));
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
            context => CreatePartition(context, permitLimit, window));
        options.OnRejected = (context, _) => HandleRejection(context, logger);
    }

    private static RateLimitPartition<string> CreatePartition(HttpContext context, int permitLimit, TimeSpan window) =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? UnknownClient,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0
            });

    private static ValueTask HandleRejection(OnRejectedContext context, ILogger logger)
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        SecurityLog.RateLimitExceeded(
            logger,
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path.Value ?? string.Empty);

        return ValueTask.CompletedTask;
    }
}
