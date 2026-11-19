using Dotnetstore.JournalSystem.Host.Logging;

namespace Dotnetstore.JournalSystem.Host.Extensions;

internal static class SecurityHeadersExtensions
{
    internal const string ScalarPath = "/scalar";
    private const string DefaultContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
    private const string ScalarContentSecurityPolicy = "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:; connect-src 'self'; frame-ancestors 'none'";

    extension(IServiceCollection services)
    {
        internal IServiceCollection AddHstsPolicy()
        {
            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.ExcludedHosts.Clear();
            });

            return services;
        }
    }

    extension(WebApplication app)
    {
        internal WebApplication UseSecurityHeaders()
        {
            app.Use((context, next) =>
            {
                var contentSecurityPolicy = context.Request.Path.StartsWithSegments(ScalarPath, StringComparison.OrdinalIgnoreCase)
                    ? ScalarContentSecurityPolicy
                    : DefaultContentSecurityPolicy;

                context.Response.OnStarting(() => ApplySecurityHeaders(context.Response.Headers, contentSecurityPolicy));

                return next(context);
            });

            return app;
        }

        internal WebApplication UseAccessDeniedLogging()
        {
            var logger = app.Services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(SecurityLog.Category);

            app.Use(async (context, next) =>
            {
                await next(context).ConfigureAwait(false);

                LogAccessDenied(logger, context);
            });

            return app;
        }
    }

    private static Task ApplySecurityHeaders(IHeaderDictionary headers, string contentSecurityPolicy)
    {
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Content-Security-Policy"] = contentSecurityPolicy;
        headers["Cache-Control"] = "no-store";

        return Task.CompletedTask;
    }

    private static void LogAccessDenied(ILogger logger, HttpContext context)
    {
        if (context.Response.StatusCode is not (StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden))
        {
            return;
        }

        SecurityLog.AccessDenied(
            logger,
            context.Response.StatusCode,
            context.Request.Method,
            context.Request.Path.Value ?? string.Empty);
    }
}
