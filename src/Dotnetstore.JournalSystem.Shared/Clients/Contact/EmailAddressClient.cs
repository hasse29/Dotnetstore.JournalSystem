using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Shared.Clients.Contact;

public interface IEmailAddressClient : ICrudClient<EditEmailAddressRequest, EditEmailAddressRequest, EmailAddressResponse>
{
    Task<ApiResult<IReadOnlyList<EmailAddressResponse>>> GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default);
}

internal sealed class EmailAddressClient(HttpClient http)
    : CrudClientBase<EditEmailAddressRequest, EditEmailAddressRequest, EmailAddressResponse>(
        http,
        ApiEndpoints.Contact.EmailAddresses.GetAll,
        ApiEndpoints.Contact.EmailAddresses.GetById,
        ApiEndpoints.Contact.EmailAddresses.Create,
        ApiEndpoints.Contact.EmailAddresses.Update,
        ApiEndpoints.Contact.EmailAddresses.Delete),
      IEmailAddressClient
{
    public Task<ApiResult<IReadOnlyList<EmailAddressResponse>>> GetByObjectIdAsync(
        Guid objectId,
        CancellationToken cancellationToken = default) =>
        GetListAsync(
            BuildUri($"{ApiEndpoints.Contact.EmailAddresses.GetAll}?objectId={objectId:D}"),
            cancellationToken);
}
