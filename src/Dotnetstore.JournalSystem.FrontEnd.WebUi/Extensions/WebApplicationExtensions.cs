using Dotnetstore.JournalSystem.Aspire.ServiceDefaults;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.Shared.Clients;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Extensions;

internal static class WebApplicationExtensions
{
    internal static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        builder.Services
            .AddHttpContextAccessor()
            .AddSingleton(TimeProvider.System)
            .AddSingleton<IAccessTokenProvider, SessionAccessTokenProvider>()
            .AddJournalApiClients(builder.Configuration)
            .AddRazorPages(options =>
            {
                options.Conventions.AllowAnonymousToPage("/Error");
                options.Conventions.AllowAnonymousToPage("/Account/Login");
                options.Conventions.AllowAnonymousToPage("/Account/ExternalLogin");
                options.Conventions.ConfigureFilter(new ServiceFilterAttribute(typeof(AccessGateFilter)));

                // Without no-store the browser serves signed-in pages from its cache on Back after sign-out.
                options.Conventions.ConfigureFilter(new ResponseCacheAttribute
                {
                    NoStore = true,
                    Location = ResponseCacheLocation.None
                });
            });

        builder.Services.AddScoped<AccessGateFilter>();
        builder.Services.AddJournalWebAuthentication(EntraSignInSettings.FromConfiguration(builder.Configuration));

        return builder;
    }

    internal static WebApplication RegisterMiddlewares(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app
                .UseExceptionHandler("/Error")
                .UseHsts();
        }

        app
            .UseHttpsRedirection()
            .UseStaticFiles()
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization();

        app
            .MapStaticAssets()
            .AllowAnonymous();

        app
            .MapRazorPages()
            .WithStaticAssets();

        app.MapDefaultEndpoints();

        return app;
    }

    internal static async Task RunApplicationAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        await app.RunAsync(cancellationToken).ConfigureAwait(false);
    }
}
