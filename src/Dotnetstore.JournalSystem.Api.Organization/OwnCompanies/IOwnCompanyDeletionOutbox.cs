namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal interface IOwnCompanyDeletionOutbox
{
    /// <summary>Deletes the company and records the event for the other modules in the same transaction.</summary>
    Task DeleteAndPublishAsync(Guid ownCompanyId, CancellationToken cancellationToken = default);
}
