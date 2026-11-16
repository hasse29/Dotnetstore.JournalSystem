using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace Dotnetstore.JournalSystem.Host.Extensions;

internal static class ForwardedHeadersExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddForwardedHeadersPolicy()
        {
            services
                .AddOptions<ForwardedHeadersOptions>()
                .Configure<IConfiguration>(ConfigureForwardedHeaders);

            return services;
        }
    }

    private static void ConfigureForwardedHeaders(ForwardedHeadersOptions options, IConfiguration configuration)
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
    }
}
