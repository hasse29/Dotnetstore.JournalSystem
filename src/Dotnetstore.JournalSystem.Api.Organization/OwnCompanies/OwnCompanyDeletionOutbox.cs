using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Shared.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal sealed class OwnCompanyDeletionOutbox(
    OrganizationDataContext dataContext,
    IServiceScopeFactory scopeFactory) : IOwnCompanyDeletionOutbox
{
    public async Task DeleteAndPublishAsync(Guid ownCompanyId, CancellationToken cancellationToken = default)
    {
        // The outbox opens its own transaction, which the retrying execution strategy only allows as one retriable unit.
        var strategy = dataContext.Database.CreateExecutionStrategy();

        await strategy
            .ExecuteAsync(token => DeleteOnceAsync(ownCompanyId, token), cancellationToken)
            .ConfigureAwait(false);
    }

    // A new scope for every attempt, so a retry does not inherit tracked entities or queued messages from the failed one.
    private async Task DeleteOnceAsync(Guid ownCompanyId, CancellationToken cancellationToken)
    {
        var scope = scopeFactory.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var outbox = scope.ServiceProvider.GetRequiredService<IDbContextOutbox<OrganizationDataContext>>();

            var company = await outbox.DbContext.OwnCompanies
                .FindAsync([ownCompanyId], cancellationToken)
                .ConfigureAwait(false);

            if (company is null)
            {
                return;
            }

            outbox.DbContext.OwnCompanies.Remove(company);

            await outbox
                .PublishAsync(new OwnCompanyDeleted(ownCompanyId))
                .ConfigureAwait(false);

            await outbox
                .SaveChangesAndFlushMessagesAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
