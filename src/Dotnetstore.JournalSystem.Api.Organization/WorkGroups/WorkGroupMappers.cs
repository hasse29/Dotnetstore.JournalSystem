using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.WorkGroups;

internal static class WorkGroupMappers
{
    internal static WorkGroupResponse ToWorkGroupResponse(this WorkGroup workGroup)
    {
        return new WorkGroupResponse(
            workGroup.Id,
            workGroup.Name,
            workGroup.Description);
    }
}
