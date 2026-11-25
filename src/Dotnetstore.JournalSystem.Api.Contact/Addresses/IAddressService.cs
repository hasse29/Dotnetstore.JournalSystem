using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal interface IAddressService
{
    Task<IEnumerable<AddressResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<AddressResponse>> GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default);
    ValueTask<Result<AddressResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<Result<AddressResponse>> CreateAsync(EditAddressRequest address, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result<AddressResponse>> UpdateAsync(Guid id, EditAddressRequest address, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
