using Dotnetstore.JournalSystem.Shared.Clients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;

internal sealed class SessionAccessTokenProvider(IHttpContextAccessor httpContextAccessor) : IAccessTokenProvider
{
    internal const string AccessTokenName = "access_token";

    public async ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null)
        {
            return null;
        }

        return await context
            .GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, AccessTokenName)
            .ConfigureAwait(false);
    }
}
