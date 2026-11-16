namespace Dotnetstore.JournalSystem.Host.Extensions;

internal static class CorsExtensions
{
    internal const string PolicyName = "DefaultCors";

    extension(IServiceCollection services)
    {
        internal IServiceCollection AddCorsPolicy(IConfiguration configuration)
        {
            var allowedOrigins = configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

            services.AddCors(options => options.AddPolicy(PolicyName, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            }));

            return services;
        }
    }
}
