using Dotnetstore.JournalSystem.Api.Organization.Authentication;
using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Seeding;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Api.Organization.AccessRequests;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;
using Dotnetstore.Security.Extensions;
using Dotnetstore.Security.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Dotnetstore.JournalSystem.Api.Organization.Extensions;

public static class HostApplicationBuilderExtensions
{
    public const string DatabaseConnectionName = "journalsystem";
    public const string EntraEnabledConfigurationKey = "Authentication:Entra:Enabled";
    public const string LoginRateLimitPolicy = "login";
    public const string TokensConfigurationSection = "Tokens";

    public static TBuilder RegisterOrganizationServices<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddSqlServerDbContext<OrganizationDataContext>(
            DatabaseConnectionName,
            configureDbContextOptions: options => options.UseSqlServer(OrganizationDataContext.ConfigureSqlServer));

        builder.Services.AddSecurity();
        builder.Services.AddTokens(options => builder.Configuration.GetSection(TokensConfigurationSection).Bind(options));

        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.AddScoped<IAccessRequestRepository, AccessRequestRepository>();
        builder.Services.AddScoped<IAccessRequestService, AccessRequestService>();
        builder.Services.AddScoped<IOwnCompanyRepository, OwnCompanyRepository>();
        builder.Services.AddScoped<IOrganizationUnitOfWork, OrganizationUnitOfWork>();
        builder.Services.AddScoped<IOwnCompanyService, OwnCompanyService>();
        builder.Services.AddScoped<IRoleRepository, RoleRepository>();
        builder.Services.AddScoped<IRoleService, RoleService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IClaimsTransformation, EntraUserClaimsTransformation>();

        if (builder.Configuration.GetValue<bool>(EntraEnabledConfigurationKey))
        {
            builder.Services.AddScoped<IEntraUserProvisioner, GraphEntraUserProvisioner>();
        }

        builder.Services.TryAddScoped<IEntraUserProvisioner, DisabledEntraUserProvisioner>();
        builder.Services.AddScoped<IWorkGroupRepository, WorkGroupRepository>();
        builder.Services.AddScoped<IWorkGroupService, WorkGroupService>();

        return builder;
    }

    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app
            .MapAccessRequestEndpoints()
            .MapAuthEndpoints()
            .MapOwnCompanyEndpoints()
            .MapRoleEndpoints()
            .MapUserEndpoints()
            .MapWorkGroupEndpoints();
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
                .MigrateAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public static async Task<bool> BootstrapOrganizationAdminAsync(
        this IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = configuration
            .GetSection(BootstrapAdminOptions.SectionName)
            .Get<BootstrapAdminOptions>();

        var scope = services.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var context = scope.ServiceProvider.GetRequiredService<OrganizationDataContext>();
            var hashHandler = scope.ServiceProvider.GetRequiredService<IHashHandler>();

            return await OrganizationBootstrapper
                .BootstrapAdminAsync(context, hashHandler, options, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public static async Task<int> SeedOrganizationDataAsync(
        this IServiceProvider services,
        IEnumerable<string> seedFilePaths,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var scope = services.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var context = scope.ServiceProvider.GetRequiredService<OrganizationDataContext>();

            return await OrganizationSeeder
                .SeedAsync(context, seedFilePaths, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
