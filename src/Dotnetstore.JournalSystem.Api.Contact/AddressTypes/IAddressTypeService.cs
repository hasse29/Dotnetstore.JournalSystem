using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.AddressTypes;

internal interface IAddressTypeService
{
    Task<IEnumerable<AddressTypeResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<Result<AddressTypeResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<Result<AddressTypeResponse>> CreateAsync(EditAddressTypeRequest addressType, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result<AddressTypeResponse>> UpdateAsync(Guid id, EditAddressTypeRequest addressType, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
