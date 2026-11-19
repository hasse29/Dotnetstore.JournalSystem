using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Contact.AddressTypes;

internal sealed class AddressTypeRepository(ContactDataContext dataContext) : GenericRepository<AddressType>(dataContext), IAddressTypeRepository;
