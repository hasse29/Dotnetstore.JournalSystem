using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal sealed class PhoneNumberRepository(ContactDataContext dataContext) : GenericRepository<PhoneNumber>(dataContext), IPhoneNumberRepository;
