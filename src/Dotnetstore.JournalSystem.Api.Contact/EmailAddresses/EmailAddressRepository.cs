using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal sealed class EmailAddressRepository(ContactDataContext dataContext) : GenericRepository<EmailAddress>(dataContext), IEmailAddressRepository
{
    async ValueTask<IReadOnlyList<EmailAddress>> IEmailAddressRepository.GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken)
    {
        return await dataContext
            .EmailAddresses
            .AsNoTracking()
            .Where(x => x.ObjectId == objectId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    async ValueTask<bool> IEmailAddressRepository.AnyWithEmailAddressTypeAsync(Guid emailAddressTypeId, CancellationToken cancellationToken)
    {
        return await dataContext
            .EmailAddresses
            .AsNoTracking()
            .AnyAsync(x => x.EmailAddressTypeId == emailAddressTypeId, cancellationToken)
            .ConfigureAwait(false);
    }
}
