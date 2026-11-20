using System.Globalization;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Dashboard;

public sealed class IndexModel(
    IUserClient users,
    IRoleClient roles,
    IWorkGroupClient workGroups,
    IOwnCompanyClient ownCompanies,
    ILogger<IndexModel> logger) : PageModel
{
    public IReadOnlyList<StatCard> Stats { get; private set; } = [];

    public IReadOnlyList<UserResponse> RecentSignIns { get; private set; } = [];

    public bool ApiUnavailable { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var usersTask = users.GetAllAsync(cancellationToken);
            var rolesTask = roles.GetAllAsync(cancellationToken);
            var workGroupsTask = workGroups.GetAllAsync(cancellationToken);
            var companiesTask = ownCompanies.GetAllAsync(cancellationToken);

            await Task.WhenAll(usersTask, rolesTask, workGroupsTask, companiesTask).ConfigureAwait(false);

            var userResult = await usersTask.ConfigureAwait(false);
            if (userResult.IsUnauthorized)
            {
                var token = await HttpContext
                    .GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, SessionAccessTokenProvider.AccessTokenName)
                    .ConfigureAwait(false);
                EntraLog.ApiRejectedToken(logger, JwtTokenSummary.Describe(token));
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
                return RedirectToPage("/Account/Login", new { error = "unauthorized" });
            }

            Stats =
            [
                new StatCard("Users", Count(userResult), "users"),
                new StatCard("Roles", Count(await rolesTask.ConfigureAwait(false)), "roles"),
                new StatCard("Work groups", Count(await workGroupsTask.ConfigureAwait(false)), "work-groups"),
                new StatCard("Companies", Count(await companiesTask.ConfigureAwait(false)), "companies")
            ];

            RecentSignIns = userResult.Value?
                .Where(static x => x.LastLoginAt is not null)
                .OrderByDescending(static x => x.LastLoginAt)
                .Take(5)
                .ToList() ?? [];
        }
        catch (HttpRequestException)
        {
            ApiUnavailable = true;
        }
        catch (TimeoutException)
        {
            ApiUnavailable = true;
        }

        return Page();
    }

    private static string Count<T>(ApiResult<IReadOnlyList<T>> result) =>
        result is { IsSuccess: true, Value: { } value }
            ? value.Count.ToString(CultureInfo.InvariantCulture)
            : "–";
}
