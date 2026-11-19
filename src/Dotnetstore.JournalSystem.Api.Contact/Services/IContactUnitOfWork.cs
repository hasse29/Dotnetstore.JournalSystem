using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;

namespace Dotnetstore.JournalSystem.Api.Contact.Services;

internal interface IContactUnitOfWork
{
    IAddressRepository Addresses { get; }
    IEmailAddressTypeRepository EmailAddressTypes { get; }
    IAddressTypeRepository AddressTypes { get; }
    void CreateTransaction();
    ValueTask CommitAsync(CancellationToken cancellationToken = default);
    ValueTask RollbackAsync(CancellationToken cancellationToken = default);
    ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
