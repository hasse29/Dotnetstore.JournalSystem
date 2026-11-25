using Dotnetstore.JournalSystem.Shared.Contracts.Events;

namespace Dotnetstore.JournalSystem.Api.Contact.Consumers;

/// <summary>Found by Wolverine's naming convention, which requires it to be public.</summary>
public static class OwnCompanyDeletedHandler
{
    public static Task HandleAsync(
        OwnCompanyDeleted message,
        IOwnCompanyContactCleanup cleanup,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(cleanup);

        return cleanup.DeleteForCompanyAsync(message.OwnCompanyId, cancellationToken);
    }
}
