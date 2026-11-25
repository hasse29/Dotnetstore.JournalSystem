using Dotnetstore.JournalSystem.Api.Contact.Services;

namespace Dotnetstore.JournalSystem.Api.Contact.Consumers;

internal sealed class OwnCompanyContactCleanup(IContactUnitOfWork contactUnitOfWork) : IOwnCompanyContactCleanup
{
    public async Task DeleteForCompanyAsync(Guid ownCompanyId, CancellationToken cancellationToken = default)
    {
        var addresses = await contactUnitOfWork.Addresses
            .GetByObjectIdAsync(ownCompanyId, cancellationToken)
            .ConfigureAwait(false);
        var emailAddresses = await contactUnitOfWork.EmailAddresses
            .GetByObjectIdAsync(ownCompanyId, cancellationToken)
            .ConfigureAwait(false);
        var phoneNumbers = await contactUnitOfWork.PhoneNumbers
            .GetByObjectIdAsync(ownCompanyId, cancellationToken)
            .ConfigureAwait(false);

        await contactUnitOfWork.Addresses
            .DeleteAsync([.. addresses.Select(x => (object)x.Id)], cancellationToken)
            .ConfigureAwait(false);
        await contactUnitOfWork.EmailAddresses
            .DeleteAsync([.. emailAddresses.Select(x => (object)x.Id)], cancellationToken)
            .ConfigureAwait(false);
        await contactUnitOfWork.PhoneNumbers
            .DeleteAsync([.. phoneNumbers.Select(x => (object)x.Id)], cancellationToken)
            .ConfigureAwait(false);

        // One save, so a failure leaves everything for the retry instead of removing only part of it.
        await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
