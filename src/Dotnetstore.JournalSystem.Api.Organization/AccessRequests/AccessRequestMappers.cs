using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.AccessRequests;

internal static class AccessRequestMappers
{
    internal static AccessRequestResponse ToAccessRequestResponse(this AccessRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (firstName, lastName) = SuggestedName.Split(request.DisplayName, request.Username);

        return new AccessRequestResponse(
            request.Id,
            request.DisplayName,
            request.Username,
            request.Status,
            request.RequestedAt,
            request.DecidedAt,
            request.DecisionNote,
            firstName,
            lastName);
    }
}
