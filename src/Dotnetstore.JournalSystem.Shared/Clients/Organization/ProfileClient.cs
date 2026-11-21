using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Shared.Clients.Organization;

public interface IProfileClient
{
    Task<ApiResult<MyProfileResponse>> GetMineAsync(CancellationToken cancellationToken = default);
}

internal sealed class ProfileClient(HttpClient http) : ApiClientBase(http), IProfileClient
{
    public Task<ApiResult<MyProfileResponse>> GetMineAsync(CancellationToken cancellationToken = default) =>
        SendAsync<MyProfileResponse>(HttpMethod.Get, BuildUri(ApiEndpoints.Organization.Users.Me), null, cancellationToken);
}
