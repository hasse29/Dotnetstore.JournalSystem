using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Shared.Clients.Contact;

public interface IEmailAddressTypeClient : ICrudClient<EditEmailAddressTypeRequest, EditEmailAddressTypeRequest, EmailAddressTypeResponse>;

internal sealed class EmailAddressTypeClient(HttpClient http)
    : CrudClientBase<EditEmailAddressTypeRequest, EditEmailAddressTypeRequest, EmailAddressTypeResponse>(
        http,
        ApiEndpoints.Contact.EmailAddressTypes.GetAll,
        ApiEndpoints.Contact.EmailAddressTypes.GetById,
        ApiEndpoints.Contact.EmailAddressTypes.Create,
        ApiEndpoints.Contact.EmailAddressTypes.Update,
        ApiEndpoints.Contact.EmailAddressTypes.Delete),
      IEmailAddressTypeClient;
