using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal interface IPhoneNumberRepository : IGenericRepository<PhoneNumber>
{
    ValueTask<IReadOnlyList<PhoneNumber>> GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default);
    ValueTask<bool> AnyWithPhoneNumberTypeAsync(Guid phoneNumberTypeId, CancellationToken cancellationToken = default);
}
