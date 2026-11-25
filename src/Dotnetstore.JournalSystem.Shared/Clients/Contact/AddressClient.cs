using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Shared.Clients.Contact;

public interface IAddressClient : ICrudClient<EditAddressRequest, EditAddressRequest, AddressResponse>
{
    Task<ApiResult<IReadOnlyList<AddressResponse>>> GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default);
}

internal sealed class AddressClient(HttpClient http)
    : CrudClientBase<EditAddressRequest, EditAddressRequest, AddressResponse>(
        http,
        ApiEndpoints.Contact.Addresses.GetAll,
        ApiEndpoints.Contact.Addresses.GetById,
        ApiEndpoints.Contact.Addresses.Create,
        ApiEndpoints.Contact.Addresses.Update,
        ApiEndpoints.Contact.Addresses.Delete),
      IAddressClient
{
    public Task<ApiResult<IReadOnlyList<AddressResponse>>> GetByObjectIdAsync(
        Guid objectId,
        CancellationToken cancellationToken = default) =>
        GetListAsync(
            BuildUri($"{ApiEndpoints.Contact.Addresses.GetAll}?objectId={objectId:D}"),
            cancellationToken);
}
