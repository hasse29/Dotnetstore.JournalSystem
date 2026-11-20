using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Shared.Clients;

public interface IAuthClient
{
    Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

internal sealed class AuthClient(HttpClient http) : ApiClientBase(http), IAuthClient
{
    public Task<ApiResult<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync<LoginResponse>(HttpMethod.Post, BuildUri(ApiEndpoints.Auth.Login), request, cancellationToken);
    }
}
