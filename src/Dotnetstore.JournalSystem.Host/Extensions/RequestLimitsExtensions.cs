using Microsoft.AspNetCore.Http.Timeouts;

namespace Dotnetstore.JournalSystem.Host.Extensions;

internal static class RequestLimitsExtensions
{
    extension(IWebHostBuilder webHost)
    {
        internal IWebHostBuilder ConfigureRequestLimits()
        {
            webHost.ConfigureKestrel((context, options) =>
            {
                options.AddServerHeader = false;
                options.Limits.MaxRequestBodySize = context.Configuration.GetValue("RequestLimits:MaxBodySizeBytes", 1_048_576L);
                options.Limits.MaxRequestHeadersTotalSize = context.Configuration.GetValue("RequestLimits:MaxHeadersTotalSizeBytes", 32_768);
                options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(context.Configuration.GetValue("RequestLimits:HeadersTimeoutSeconds", 15));
            });

            return webHost;
        }
    }

    extension(IServiceCollection services)
    {
        internal IServiceCollection AddRequestTimeoutPolicy()
        {
            services.AddRequestTimeouts(_ => { });

            services
                .AddOptions<RequestTimeoutOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                    options.DefaultPolicy = new RequestTimeoutPolicy
                    {
                        Timeout = TimeSpan.FromSeconds(configuration.GetValue("RequestLimits:RequestTimeoutSeconds", 30))
                    });

            return services;
        }
    }
}
