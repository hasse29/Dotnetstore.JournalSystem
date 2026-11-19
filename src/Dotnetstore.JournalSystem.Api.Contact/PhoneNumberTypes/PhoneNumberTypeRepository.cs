using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;

internal sealed class PhoneNumberTypeRepository(ContactDataContext dataContext) : GenericRepository<PhoneNumberType>(dataContext), IPhoneNumberTypeRepository;
