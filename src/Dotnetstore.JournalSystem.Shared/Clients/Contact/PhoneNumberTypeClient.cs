using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Shared.Clients.Contact;

public interface IPhoneNumberTypeClient : ICrudClient<EditPhoneNumberTypeRequest, EditPhoneNumberTypeRequest, PhoneNumberTypeResponse>;

internal sealed class PhoneNumberTypeClient(HttpClient http)
    : CrudClientBase<EditPhoneNumberTypeRequest, EditPhoneNumberTypeRequest, PhoneNumberTypeResponse>(
        http,
        ApiEndpoints.Contact.PhoneNumberTypes.GetAll,
        ApiEndpoints.Contact.PhoneNumberTypes.GetById,
        ApiEndpoints.Contact.PhoneNumberTypes.Create,
        ApiEndpoints.Contact.PhoneNumberTypes.Update,
        ApiEndpoints.Contact.PhoneNumberTypes.Delete),
      IPhoneNumberTypeClient;
