using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;

internal sealed class EmailAddressTypeRepository(ContactDataContext dataContext) : GenericRepository<EmailAddressType>(dataContext), IEmailAddressTypeRepository;
