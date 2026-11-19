using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;

namespace Dotnetstore.JournalSystem.Api.Contact.Services;

internal interface IContactUnitOfWork
{
    IAddressTypeRepository AddressTypes { get; }
    void CreateTransaction();
    ValueTask CommitAsync(CancellationToken cancellationToken = default);
    ValueTask RollbackAsync(CancellationToken cancellationToken = default);
    ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
