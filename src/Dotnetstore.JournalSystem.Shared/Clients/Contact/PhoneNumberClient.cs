using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Shared.Clients.Contact;

public interface IPhoneNumberClient : ICrudClient<EditPhoneNumberRequest, EditPhoneNumberRequest, PhoneNumberResponse>
{
    Task<ApiResult<IReadOnlyList<PhoneNumberResponse>>> GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default);
}

internal sealed class PhoneNumberClient(HttpClient http)
    : CrudClientBase<EditPhoneNumberRequest, EditPhoneNumberRequest, PhoneNumberResponse>(
        http,
        ApiEndpoints.Contact.PhoneNumbers.GetAll,
        ApiEndpoints.Contact.PhoneNumbers.GetById,
        ApiEndpoints.Contact.PhoneNumbers.Create,
        ApiEndpoints.Contact.PhoneNumbers.Update,
        ApiEndpoints.Contact.PhoneNumbers.Delete),
      IPhoneNumberClient
{
    public Task<ApiResult<IReadOnlyList<PhoneNumberResponse>>> GetByObjectIdAsync(
        Guid objectId,
        CancellationToken cancellationToken = default) =>
        GetListAsync(
            BuildUri($"{ApiEndpoints.Contact.PhoneNumbers.GetAll}?objectId={objectId:D}"),
            cancellationToken);
}
