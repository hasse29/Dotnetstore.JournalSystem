using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;

internal sealed class AccessGateFilter(IAccessRequestClient requests, ILogger<AccessGateFilter> logger) : IAsyncPageFilter
{
    private const string StatusPage = "/Access/Status";

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var http = context.HttpContext;

        if (http.User.Identity?.IsAuthenticated != true ||
            AccessGate.IsExempt(context.ActionDescriptor.ViewEnginePath) ||
            AccessGate.HasApplicationAccount(http.User))
        {
            await next().ConfigureAwait(false);
            return;
        }

        ApiResult<AccessRequestResponse> result;
        try
        {
            result = await requests.GetMineAsync(http.RequestAborted).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            context.Result = new RedirectToPageResult(StatusPage);
            return;
        }
        catch (TimeoutException)
        {
            context.Result = new RedirectToPageResult(StatusPage);
            return;
        }

        switch (AccessGate.Decide(result))
        {
            case AccessGateOutcome.Allow:
                await next().ConfigureAwait(false);
                break;

            case AccessGateOutcome.SignOut:
                var token = await http
                    .GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, SessionAccessTokenProvider.AccessTokenName)
                    .ConfigureAwait(false);
                EntraLog.ApiRejectedToken(logger, JwtTokenSummary.Describe(token));
                await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
                context.Result = new RedirectToPageResult("/Account/Login", new { error = "unauthorized" });
                break;

            default:
                context.Result = new RedirectToPageResult(StatusPage);
                break;
        }
    }
}
