using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal sealed class AddressRepository(ContactDataContext dataContext) : GenericRepository<Address>(dataContext), IAddressRepository;
