using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Dotnetstore.JournalSystem.Host.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http.Timeouts;

namespace Dotnetstore.JournalSystem.Host.Extensions;

internal static class WebApplicationExtensions
{
    internal const string CorsPolicyName = "DefaultCors";
    internal const string HealthPath = "/health";

    internal static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            options.AddServerHeader = false;
            options.Limits.MaxRequestBodySize = context.Configuration.GetValue("RequestLimits:MaxBodySizeBytes", 1_048_576L);
            options.Limits.MaxRequestHeadersTotalSize = context.Configuration.GetValue("RequestLimits:MaxHeadersTotalSizeBytes", 32_768);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(context.Configuration.GetValue("RequestLimits:HeadersTimeoutSeconds", 15));
        });

        builder.Services
            .AddOpenApi()
            .AddProblemDetails()
            .AddHealthChecks();

        builder.Services
            .AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.ExcludedHosts.Clear();
            })
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
            .AddRateLimiter(_ => { })
            .AddRequestTimeouts(_ => { });

        builder.Services
            .AddOptions<ForwardedHeadersOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                var proxies = configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
                foreach (var proxy in proxies)
                {
                    if (IPAddress.TryParse(proxy, out var address))
                    {
                        options.KnownProxies.Add(address);
                    }
                }
            });

        builder.Services
            .AddOptions<RequestTimeoutOptions>()
            .Configure<IConfiguration>((options, configuration) =>
                options.DefaultPolicy = new RequestTimeoutPolicy
                {
                    Timeout = TimeSpan.FromSeconds(configuration.GetValue("RequestLimits:RequestTimeoutSeconds", 30))
                });

        builder.Services
            .AddOptions<RateLimiterOptions>()
            .Configure<IConfiguration, ILoggerFactory>((options, configuration, loggerFactory) =>
            {
                var permitLimit = configuration.GetValue("RateLimiting:PermitLimit", 100);
                var windowSeconds = configuration.GetValue("RateLimiting:WindowSeconds", 60);
                var logger = loggerFactory.CreateLogger("Dotnetstore.JournalSystem.Security");

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

                    SecurityLog.RateLimitExceeded(
                        logger,
                        context.HttpContext.Request.Method,
                        context.HttpContext.Request.Path.Value ?? string.Empty);

                    return ValueTask.CompletedTask;
                };
            });

        return builder;
    }

    extension(WebApplication app)
    {
        internal WebApplication RegisterPipeline()
        {
            app
                .UseForwardedHeaders()
                .UseExceptionHandler()
                .UseStatusCodePages();

            if (!app.Environment.IsDevelopment())
            {
                app
                    .UseHsts();
            }

            app
                .UseHttpsRedirection();

            app
                .UseSecurityHeaders()
                .UseAccessDeniedLogging();

            app
                .UseCors(CorsPolicyName)
                .UseRequestTimeouts()
                .UseRateLimiter();

            if (app.Environment.IsDevelopment())
            {
                app
                    .MapOpenApi();
            }

            app
                .MapHealthChecks(HealthPath);

            return app;
        }

        internal async Task RunApplicationAsync(CancellationToken cancellationToken = default)
        {
            await app
                .RunAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        private WebApplication UseSecurityHeaders()
        {
            app.Use((context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    var headers = context.Response.Headers;
                    headers["X-Content-Type-Options"] = "nosniff";
                    headers["X-Frame-Options"] = "DENY";
                    headers["Referrer-Policy"] = "no-referrer";
                    headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
                    headers["Cache-Control"] = "no-store";
                    return Task.CompletedTask;
                });

                return next(context);
            });

            return app;
        }

        private WebApplication UseAccessDeniedLogging()
        {
            var logger = app.Services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("Dotnetstore.JournalSystem.Security");

            app.Use(async (context, next) =>
            {
                await next(context).ConfigureAwait(false);

                if (context.Response.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
                {
                    SecurityLog.AccessDenied(
                        logger,
                        context.Response.StatusCode,
                        context.Request.Method,
                        context.Request.Path.Value ?? string.Empty);
                }
            });

            return app;
        }
    }
}
