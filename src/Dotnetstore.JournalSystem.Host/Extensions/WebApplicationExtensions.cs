namespace Dotnetstore.JournalSystem.Host.Extensions;

internal static class WebApplicationExtensions
{
    internal static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOpenApi();
        
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
            .UseHttpsRedirection();
        
        return app;
    }
    
    internal static async Task RunApplicationAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        await app
            .RunAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}