using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dotnetstore.JournalSystem.Api.Organization.Extensions;

public static class HostApplicationBuilderExtensions
{
    public const string DatabaseConnectionName = "journalsystem";

    public static TBuilder RegisterOrganizationServices<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddSqlServerDbContext<OrganizationDataContext>(DatabaseConnectionName);

        builder.Services.AddScoped<IOwnCompanyRepository, OwnCompanyRepository>();
        builder.Services.AddScoped<IOrganizationUnitOfWork, OrganizationUnitOfWork>();
        builder.Services.AddScoped<IOwnCompanyService, OwnCompanyService>();

        return builder;
    }

    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.MapOwnCompanyEndpoints();
    }

    public static async Task InitializeOrganizationDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var scope = services.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var context = scope.ServiceProvider.GetRequiredService<OrganizationDataContext>();

            await context.Database
                .EnsureCreatedAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
