using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal interface IEmailAddressRepository : IGenericRepository<EmailAddress>
{
    ValueTask<bool> AnyWithEmailAddressTypeAsync(Guid emailAddressTypeId, CancellationToken cancellationToken = default);
}
