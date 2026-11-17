using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal sealed class OwnCompanyRepository(OrganizationDataContext dataContext) : GenericRepository<OwnCompany>(dataContext), IOwnCompanyRepository;