using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;

namespace Dotnetstore.JournalSystem.Api.Contact.Services;

internal interface IContactUnitOfWork
{
    IAddressRepository Addresses { get; }
    IEmailAddressTypeRepository EmailAddressTypes { get; }
    IEmailAddressRepository EmailAddresses { get; }
    IPhoneNumberTypeRepository PhoneNumberTypes { get; }
    IPhoneNumberRepository PhoneNumbers { get; }
    IAddressTypeRepository AddressTypes { get; }
    void CreateTransaction();
    ValueTask CommitAsync(CancellationToken cancellationToken = default);
    ValueTask RollbackAsync(CancellationToken cancellationToken = default);
    ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
