using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;

internal interface IEmailAddressTypeService
{
    Task<IEnumerable<EmailAddressTypeResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<Result<EmailAddressTypeResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<Result<EmailAddressTypeResponse>> CreateAsync(EditEmailAddressTypeRequest emailAddressType, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result<EmailAddressTypeResponse>> UpdateAsync(Guid id, EditEmailAddressTypeRequest emailAddressType, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
