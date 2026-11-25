using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal sealed class AddressRepository(ContactDataContext dataContext) : GenericRepository<Address>(dataContext), IAddressRepository
{
    async ValueTask<IReadOnlyList<Address>> IAddressRepository.GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken)
    {
        return await dataContext
            .Addresses
            .AsNoTracking()
            .Where(x => x.ObjectId == objectId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    async ValueTask<bool> IAddressRepository.AnyWithAddressTypeAsync(Guid addressTypeId, CancellationToken cancellationToken)
    {
        return await dataContext
            .Addresses
            .AsNoTracking()
            .AnyAsync(x => x.AddressTypeId == addressTypeId, cancellationToken)
            .ConfigureAwait(false);
    }
}
