using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal interface IEmailAddressService
{
    Task<IEnumerable<EmailAddressResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailAddressResponse>> GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default);
    ValueTask<Result<EmailAddressResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<Result<EmailAddressResponse>> CreateAsync(EditEmailAddressRequest emailAddress, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result<EmailAddressResponse>> UpdateAsync(Guid id, EditEmailAddressRequest emailAddress, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
