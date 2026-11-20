namespace Dotnetstore.JournalSystem.Shared.Clients;

/// <summary>
/// Supplies the bearer token the clients send to the API. The frontend implements this (for example from the signed-in
/// user's session) and registers it before <c>AddJournalApiClients</c>.
/// </summary>
/// <remarks>
/// IHttpClientFactory builds message handlers in their own DI scope, not the web request's scope. Implementations must
/// therefore get per-request state from <c>IHttpContextAccessor</c> (or similar ambient context) and must not rely on
/// scoped services that were filled in during the request.
/// </remarks>
public interface IAccessTokenProvider
{
    /// <summary>Returns the access token to send, or null/empty to call the API anonymously.</summary>
    ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
