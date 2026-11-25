using System.Data.Common;
using Dotnetstore.JournalSystem.Api.Contact.Consumers;
using JasperFx;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Persistence.Durability;
using Wolverine.SqlServer;
using OrganizationExtensions = Dotnetstore.JournalSystem.Api.Organization.Extensions.HostApplicationBuilderExtensions;

namespace Dotnetstore.JournalSystem.Host.Extensions;

/// <summary>Messaging between the modules: Wolverine with its inbox and outbox stored in SQL Server.</summary>
internal static class MessagingExtensions
{
    internal const string SchemaName = "wolverine";

    extension(WebApplicationBuilder builder)
    {
        internal WebApplicationBuilder AddJournalMessaging()
        {
            var connectionString = builder.Configuration.GetConnectionString(OrganizationExtensions.DatabaseConnectionName);

            builder.Host.UseWolverine(options =>
            {
                options.ApplicationAssembly = typeof(Program).Assembly;
                options.Discovery.IncludeAssembly(typeof(OwnCompanyDeletedHandler).Assembly);
                options.CodeGeneration.AlwaysUseServiceLocationFor<IOwnCompanyContactCleanup>();
                // The tables are created only where the migrations run. The host needs them, and the database, to start.
                options.AutoBuildMessageStorageOnStartup = AutoCreate.None;

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    options.PersistMessagesWithSqlServer(connectionString, SchemaName);
                    options.UseEntityFrameworkCoreTransactions();
                    options.Policies.UseDurableLocalQueues();
                }

                // Quick retries for a passing fault, then spaced out ones. After that the message is dead-lettered.
                options.OnException<DbException>()
                    .Or<DbUpdateException>()
                    .Or<TimeoutException>()
                    .RetryWithCooldown(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
                    .Then.ScheduleRetry(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(10), TimeSpan.FromHours(1));
            });

            return builder;
        }
    }

    /// <summary>Creates or updates the message tables. Runs wherever the module migrations run.</summary>
    internal static async Task InitializeMessagingDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await services
            .GetRequiredService<IMessageStore>()
            .Admin
            .MigrateAsync()
            .ConfigureAwait(false);
    }
}
