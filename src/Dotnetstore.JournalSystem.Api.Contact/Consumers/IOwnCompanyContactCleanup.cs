namespace Dotnetstore.JournalSystem.Api.Contact.Consumers;

/// <summary>Public only because Wolverine's generated handler code has to name it.</summary>
public interface IOwnCompanyContactCleanup
{
    Task DeleteForCompanyAsync(Guid ownCompanyId, CancellationToken cancellationToken = default);
}
