using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal sealed class RoleRepository(OrganizationDataContext dataContext) : GenericRepository<Role>(dataContext), IRoleRepository;
