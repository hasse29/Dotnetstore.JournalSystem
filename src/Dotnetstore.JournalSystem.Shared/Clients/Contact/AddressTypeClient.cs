using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Shared.Clients.Contact;

public interface IAddressTypeClient : ICrudClient<EditAddressTypeRequest, EditAddressTypeRequest, AddressTypeResponse>;

internal sealed class AddressTypeClient(HttpClient http)
    : CrudClientBase<EditAddressTypeRequest, EditAddressTypeRequest, AddressTypeResponse>(
        http,
        ApiEndpoints.Contact.AddressTypes.GetAll,
        ApiEndpoints.Contact.AddressTypes.GetById,
        ApiEndpoints.Contact.AddressTypes.Create,
        ApiEndpoints.Contact.AddressTypes.Update,
        ApiEndpoints.Contact.AddressTypes.Delete),
      IAddressTypeClient;
