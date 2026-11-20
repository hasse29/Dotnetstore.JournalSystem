using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Shared.Clients.Contact;

public interface IEmailAddressClient : ICrudClient<EditEmailAddressRequest, EditEmailAddressRequest, EmailAddressResponse>;

internal sealed class EmailAddressClient(HttpClient http)
    : CrudClientBase<EditEmailAddressRequest, EditEmailAddressRequest, EmailAddressResponse>(
        http,
        ApiEndpoints.Contact.EmailAddresses.GetAll,
        ApiEndpoints.Contact.EmailAddresses.GetById,
        ApiEndpoints.Contact.EmailAddresses.Create,
        ApiEndpoints.Contact.EmailAddresses.Update,
        ApiEndpoints.Contact.EmailAddresses.Delete),
      IEmailAddressClient;
