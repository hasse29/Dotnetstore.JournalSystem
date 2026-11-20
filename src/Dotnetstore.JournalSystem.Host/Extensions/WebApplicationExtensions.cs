using Dotnetstore.JournalSystem.Api.Contact.Extensions;
using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.Aspire.ServiceDefaults;
using Scalar.AspNetCore;

namespace Dotnetstore.JournalSystem.Host.Extensions;

internal static class WebApplicationExtensions
{
    internal static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        builder
            .AddServiceDefaults()
            .RegisterOrganizationServices()
            .RegisterContactServices()
            .AddJournalAuthentication();

        builder.WebHost
            .ConfigureRequestLimits();

        builder.Services
            .AddOpenApi()
            .AddProblemDetails()
            .AddHealthChecks();

        builder.Services
            .AddHstsPolicy()
            .AddCorsPolicy(builder.Configuration)
            .AddRateLimiting()
            .AddRequestTimeoutPolicy()
            .AddForwardedHeadersPolicy();

        return builder;
    }

    extension(WebApplication app)
    {
        internal WebApplication RegisterPipeline()
        {
            app
                .UseErrorHandling()
                .UseTransportSecurity()
                .UseRequestProtection()
                .UseJournalAuthentication()
                .MapApplicationEndpoints();

            return app;
        }

        internal async Task RunApplicationAsync(CancellationToken cancellationToken = default)
        {
            if (app.Configuration.GetValue("Database:MigrateOnStartup", false))
            {
                await app.Services
                    .InitializeOrganizationDatabaseAsync(cancellationToken)
                    .ConfigureAwait(false);

                await app.Services
                    .InitializeContactDatabaseAsync(cancellationToken)
                    .ConfigureAwait(false);

                var seedFiles = (app.Configuration.GetSection("Seeding:Paths").Get<string[]>()
                        ?? ["../../seed/seed.json", "../../seed/seed.local.json"])
                    .Select(path => Path.Combine(app.Environment.ContentRootPath, path));

                await app.Services
                    .SeedOrganizationDataAsync(seedFiles, cancellationToken)
                    .ConfigureAwait(false);

                await app.Services
                    .SeedContactDataAsync(seedFiles, cancellationToken)
                    .ConfigureAwait(false);

                await app.Services
                    .BootstrapOrganizationAdminAsync(app.Configuration, cancellationToken)
                    .ConfigureAwait(false);
            }

            await app
                .RunAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        private WebApplication UseErrorHandling()
        {
            app
                .UseForwardedHeaders()
                .UseExceptionHandler()
                .UseStatusCodePages();

            return app;
        }

        private WebApplication UseTransportSecurity()
        {
            if (!app.Environment.IsDevelopment())
            {
                app
                    .UseHsts();
            }

            app
                .UseHttpsRedirection();

            return app;
        }

        private WebApplication UseRequestProtection()
        {
            app
                .UseSecurityHeaders()
                .UseAccessDeniedLogging()
                .UseCors(CorsExtensions.PolicyName)
                .UseRequestTimeouts()
                .UseRateLimiter();

            return app;
        }

        private WebApplication MapApplicationEndpoints()
        {
            if (app.Environment.IsDevelopment())
            {
                app
                    .MapOpenApi();
                app
                    .MapScalarApiReference();
            }

            app
                .MapOrganizationEndpoints()
                .MapContactEndpoints();

            app
                .MapHealthEndpoints();

            return app;
        }
    }
}
