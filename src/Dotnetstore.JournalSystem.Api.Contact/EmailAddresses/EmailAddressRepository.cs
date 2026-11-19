using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal sealed class EmailAddressRepository(ContactDataContext dataContext) : GenericRepository<EmailAddress>(dataContext), IEmailAddressRepository;
