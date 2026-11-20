using System.ComponentModel.DataAnnotations;
using System.Net;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel(IAuthClient authClient, EntraSignInSettings entra) : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public bool EntraEnabled => entra.Enabled;

    [BindProperty(SupportsGet = true)]
    public string? Error { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Code { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(SafeReturnUrl());
        }

        if (Error == "external")
        {
            var detail = EntraFailureCode.IsDisplayable(Code) ? $" ({Code})" : string.Empty;
            ModelState.AddModelError(string.Empty, $"Microsoft sign-in did not complete{detail}. Please try again.");
        }

        if (Error == "unauthorized")
        {
            ModelState.AddModelError(
                string.Empty,
                "You signed in, but the API did not accept your sign-in (401). Check the api log for the reason.");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var username = Input.Username.Trim();

        ApiResult<LoginResponse> result;
        try
        {
            result = await authClient
                .LoginAsync(new LoginRequest(username, Input.Password), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            return ShowError("The service is unavailable right now. Please try again later.");
        }
        catch (TimeoutException)
        {
            return ShowError("The service did not respond in time. Please try again.");
        }

        if (result is { IsSuccess: true, Value: { } login })
        {
            var principal = JwtClaimsReader.CreatePrincipal(
                login.AccessToken,
                username,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var properties = new AuthenticationProperties
            {
                ExpiresUtc = login.ExpiresAt,
                IsPersistent = false,
                AllowRefresh = false
            };
            properties.StoreTokens(
            [
                new AuthenticationToken { Name = SessionAccessTokenProvider.AccessTokenName, Value = login.AccessToken }
            ]);

            await HttpContext
                .SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties)
                .ConfigureAwait(false);

            return LocalRedirect(SafeReturnUrl());
        }

        return ShowError(result.StatusCode switch
        {
            HttpStatusCode.TooManyRequests => "Too many attempts. Please wait a moment and try again.",
            HttpStatusCode.Forbidden => "This account is locked. Please try again later.",
            HttpStatusCode.ServiceUnavailable => "The service is unavailable right now. Please try again later.",
            _ => "Invalid username or password."
        });
    }

    private PageResult ShowError(string message)
    {
        Input.Password = string.Empty;
        ModelState.AddModelError(string.Empty, message);
        return Page();
    }

    private string SafeReturnUrl() =>
        !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/Dashboard";

}
