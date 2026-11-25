using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal interface IPhoneNumberService
{
    Task<IEnumerable<PhoneNumberResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PhoneNumberResponse>> GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default);
    ValueTask<Result<PhoneNumberResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<Result<PhoneNumberResponse>> CreateAsync(EditPhoneNumberRequest phoneNumber, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result<PhoneNumberResponse>> UpdateAsync(Guid id, EditPhoneNumberRequest phoneNumber, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
