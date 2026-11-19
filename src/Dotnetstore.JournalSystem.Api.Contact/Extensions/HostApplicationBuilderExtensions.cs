using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dotnetstore.JournalSystem.Api.Contact.Extensions;

public static class HostApplicationBuilderExtensions
{
    public const string DatabaseConnectionName = "journalsystem";

    public static TBuilder RegisterContactServices<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddSqlServerDbContext<ContactDataContext>(DatabaseConnectionName);

        builder.Services.AddScoped<IAddressTypeRepository, AddressTypeRepository>();
        builder.Services.AddScoped<IContactUnitOfWork, ContactUnitOfWork>();
        builder.Services.AddScoped<IAddressTypeService, AddressTypeService>();
        builder.Services.AddScoped<IAddressRepository, AddressRepository>();
        builder.Services.AddScoped<IAddressService, AddressService>();
        builder.Services.AddScoped<IEmailAddressTypeRepository, EmailAddressTypeRepository>();
        builder.Services.AddScoped<IEmailAddressTypeService, EmailAddressTypeService>();
        builder.Services.AddScoped<IEmailAddressRepository, EmailAddressRepository>();
        builder.Services.AddScoped<IEmailAddressService, EmailAddressService>();

        return builder;
    }

    public static IEndpointRouteBuilder MapContactEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app
            .MapAddressTypeEndpoints()
            .MapAddressEndpoints()
            .MapEmailAddressTypeEndpoints()
            .MapEmailAddressEndpoints();
    }

    public static async Task InitializeContactDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var scope = services.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var context = scope.ServiceProvider.GetRequiredService<ContactDataContext>();

            // The database is shared with other modules, and EnsureCreated does nothing once any
            // module has created its tables, so this module's tables are created explicitly.
            await context.Database
                .EnsureCreatedAsync(cancellationToken)
                .ConfigureAwait(false);

            var tableCount = await context.Database
                .SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM sys.tables AS t INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id WHERE s.name = {DataSchemeConstants.ContactSchemeName}")
                .SingleAsync(cancellationToken)
                .ConfigureAwait(false);

            if (tableCount == 0)
            {
                await context
                    .GetService<IRelationalDatabaseCreator>()
                    .CreateTablesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
