using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Organization.Authentication;

internal static class AuthEndpoints
{
    internal static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app
            .MapPost(ApiEndpoints.Auth.Login, LoginAsync)
            .WithTags("Authentication")
            .WithName("Login")
            .WithSummary("Signs in a local user and returns an access token.")
            .AllowAnonymous()
            .RequireRateLimiting(HostApplicationBuilderExtensions.LoginRateLimitPolicy);

        return app;
    }

    private static async Task<Results<Ok<LoginResponse>, UnauthorizedHttpResult, ValidationProblem>> LoginAsync(
        LoginRequest? request,
        IAuthService service,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var result = await service
            .LoginAsync(request!, cancellationToken)
            .ConfigureAwait(false);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Unauthorized();
    }

    internal static Dictionary<string, string[]> Validate(LoginRequest? request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors[nameof(request)] = ["A request body is required."];
            return errors;
        }

        if (string.IsNullOrWhiteSpace(request.Username) ||
            request.Username.Length > DataSchemeConstants.UserUsernameMaxLength)
        {
            errors[nameof(request.Username)] = [$"{nameof(request.Username)} is required."];
        }

        if (string.IsNullOrEmpty(request.Password) ||
            request.Password.Length > DataSchemeConstants.UserPasswordMaxLength)
        {
            errors[nameof(request.Password)] = [$"{nameof(request.Password)} is required."];
        }

        return errors;
    }
}
