using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal sealed class PhoneNumberRepository(ContactDataContext dataContext) : GenericRepository<PhoneNumber>(dataContext), IPhoneNumberRepository
{
    async ValueTask<IReadOnlyList<PhoneNumber>> IPhoneNumberRepository.GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken)
    {
        return await dataContext
            .PhoneNumbers
            .AsNoTracking()
            .Where(x => x.ObjectId == objectId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    async ValueTask<bool> IPhoneNumberRepository.AnyWithPhoneNumberTypeAsync(Guid phoneNumberTypeId, CancellationToken cancellationToken)
    {
        return await dataContext
            .PhoneNumbers
            .AsNoTracking()
            .AnyAsync(x => x.PhoneNumberTypeId == phoneNumberTypeId, cancellationToken)
            .ConfigureAwait(false);
    }
}
