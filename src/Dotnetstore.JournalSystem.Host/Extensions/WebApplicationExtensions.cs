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
            }));

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
            .UseCors(CorsPolicyName);

        return app;
    }

    internal static async Task RunApplicationAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        await app
            .RunAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
