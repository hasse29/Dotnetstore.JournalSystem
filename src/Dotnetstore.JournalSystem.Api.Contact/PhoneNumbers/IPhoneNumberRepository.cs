using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal interface IPhoneNumberRepository : IGenericRepository<PhoneNumber>
{
    ValueTask<bool> AnyWithPhoneNumberTypeAsync(Guid phoneNumberTypeId, CancellationToken cancellationToken = default);
}
