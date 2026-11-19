using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;

internal interface IPhoneNumberTypeService
{
    Task<IEnumerable<PhoneNumberTypeResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<Result<PhoneNumberTypeResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<Result<PhoneNumberTypeResponse>> CreateAsync(EditPhoneNumberTypeRequest phoneNumberType, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result<PhoneNumberTypeResponse>> UpdateAsync(Guid id, EditPhoneNumberTypeRequest phoneNumberType, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
