using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Organization.WorkGroups;

internal sealed class WorkGroupRepository(OrganizationDataContext dataContext) : GenericRepository<WorkGroup>(dataContext), IWorkGroupRepository;
