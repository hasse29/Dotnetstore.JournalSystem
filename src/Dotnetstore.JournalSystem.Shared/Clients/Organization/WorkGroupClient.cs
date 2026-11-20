using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Shared.Clients.Organization;

public interface IWorkGroupClient : ICrudClient<EditWorkGroupRequest, EditWorkGroupRequest, WorkGroupResponse>;

internal sealed class WorkGroupClient(HttpClient http)
    : CrudClientBase<EditWorkGroupRequest, EditWorkGroupRequest, WorkGroupResponse>(
        http,
        ApiEndpoints.Organization.WorkGroups.GetAll,
        ApiEndpoints.Organization.WorkGroups.GetById,
        ApiEndpoints.Organization.WorkGroups.Create,
        ApiEndpoints.Organization.WorkGroups.Update,
        ApiEndpoints.Organization.WorkGroups.Delete),
      IWorkGroupClient;
