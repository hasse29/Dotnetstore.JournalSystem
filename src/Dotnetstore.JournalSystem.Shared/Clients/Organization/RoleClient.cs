using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Shared.Clients.Organization;

public interface IRoleClient : ICrudClient<EditRoleRequest, EditRoleRequest, RoleResponse>;

internal sealed class RoleClient(HttpClient http)
    : CrudClientBase<EditRoleRequest, EditRoleRequest, RoleResponse>(
        http,
        ApiEndpoints.Organization.Roles.GetAll,
        ApiEndpoints.Organization.Roles.GetById,
        ApiEndpoints.Organization.Roles.Create,
        ApiEndpoints.Organization.Roles.Update,
        ApiEndpoints.Organization.Roles.Delete),
      IRoleClient;
