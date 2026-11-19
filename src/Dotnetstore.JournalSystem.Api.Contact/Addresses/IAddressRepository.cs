using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal interface IAddressRepository : IGenericRepository<Address>
{
    ValueTask<bool> AnyWithAddressTypeAsync(Guid addressTypeId, CancellationToken cancellationToken = default);
}
