using System.Security.Claims;
using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Authentication;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal static class UserEndpoints
{
    private const string GetByIdName = "GetUserById";

    internal static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var mine = app
            .MapGroup(string.Empty)
            .WithTags("Users")
            .RequireAuthorization(AuthPolicies.Authenticated);

        mine
            .MapGet(ApiEndpoints.Organization.Users.Me, GetMyProfileAsync)
            .WithName("GetMyProfile")
            .WithSummary("Gets the signed-in user's own profile, with the personal identity number masked.");

        var group = app
            .MapGroup(string.Empty)
            .WithTags("Users")
            .RequireAuthorization(AuthPolicies.Administrator);

        group
            .MapGet(ApiEndpoints.Organization.Users.GetAll, GetAllAsync)
            .WithName("GetUsers")
            .WithSummary("Gets all users.");

        group
            .MapGet(ApiEndpoints.Organization.Users.GetById, GetByIdAsync)
            .WithName(GetByIdName)
            .WithSummary("Gets a user by id.");

        group
            .MapPost(ApiEndpoints.Organization.Users.Create, CreateAsync)
            .WithName("CreateUser")
            .WithSummary("Creates a local or Entra ID user. For Entra users the response carries a one-time temporary password.");

        group
            .MapPut(ApiEndpoints.Organization.Users.Update, UpdateAsync)
            .WithName("UpdateUser")
            .WithSummary("Updates a user.");

        group
            .MapDelete(ApiEndpoints.Organization.Users.Delete, DeleteAsync)
            .WithName("DeleteUser")
            .WithSummary("Deletes a user.");

        return app;
    }

    private static async Task<Results<Ok<MyProfileResponse>, NotFound, ProblemHttpResult>> GetMyProfileAsync(
        ClaimsPrincipal principal,
        IUserService service,
        CancellationToken cancellationToken)
    {
        var userId = principal.GetUserId();
        if (userId is null)
        {
            return TypedResults.NotFound();
        }

        var result = await service
            .GetProfileAsync(userId.Value, cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(result.Value),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Ok<UserResponse[]>> GetAllAsync(
        IUserService service,
        CancellationToken cancellationToken)
    {
        var items = await service
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(items.ToArray());
    }

    private static async Task<Results<Ok<UserResponse>, NotFound, ProblemHttpResult>> GetByIdAsync(
        Guid id,
        IUserService service,
        CancellationToken cancellationToken)
    {
        var result = await service
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(result.Value),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Results<CreatedAtRoute<UserResponse>, ValidationProblem, Conflict<string>, ForbidHttpResult, ProblemHttpResult>> CreateAsync(
        CreateUserRequest? request,
        ClaimsPrincipal principal,
        IUserService service,
        CancellationToken cancellationToken)
    {
        var errors = UserRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var result = await service
            .CreateAsync(request!, principal.GetUserId(), cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.CreatedAtRoute(result.Value, GetByIdName, new RouteValueDictionary { ["id"] = result.Value.Id }),
            ResultStatus.Invalid => TypedResults.ValidationProblem(ToErrors(result.ValidationErrors)),
            ResultStatus.Conflict => TypedResults.Conflict(result.Errors.FirstOrDefault() ?? "Conflict."),
            ResultStatus.Forbidden => TypedResults.Forbid(),
            ResultStatus.Unavailable => TypedResults.Problem(result.Errors.FirstOrDefault(), statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Results<Ok<UserResponse>, NotFound, ValidationProblem, ForbidHttpResult, ProblemHttpResult>> UpdateAsync(
        Guid id,
        EditUserRequest? request,
        ClaimsPrincipal principal,
        IUserService service,
        CancellationToken cancellationToken)
    {
        var errors = UserRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var result = await service
            .UpdateAsync(id, request!, principal.GetUserId(), cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(result.Value),
            ResultStatus.NotFound => TypedResults.NotFound(),
            ResultStatus.Invalid => TypedResults.ValidationProblem(ToErrors(result.ValidationErrors)),
            ResultStatus.Forbidden => TypedResults.Forbid(),
            ResultStatus.Unavailable => TypedResults.Problem(result.Errors.FirstOrDefault(), statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Results<NoContent, NotFound, Conflict<string>, ForbidHttpResult, ProblemHttpResult>> DeleteAsync(
        Guid id,
        ClaimsPrincipal principal,
        IUserService service,
        CancellationToken cancellationToken)
    {
        if (principal.GetUserId() == id)
        {
            return TypedResults.Conflict("You cannot delete your own account.");
        }

        var result = await service
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.NoContent(),
            ResultStatus.NotFound => TypedResults.NotFound(),
            ResultStatus.Conflict => TypedResults.Conflict(result.Errors.FirstOrDefault() ?? "Conflict."),
            ResultStatus.Forbidden => TypedResults.Forbid(),
            ResultStatus.Unavailable => TypedResults.Problem(result.Errors.FirstOrDefault(), statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => TypedResults.Problem()
        };
    }

    private static Dictionary<string, string[]> ToErrors(IEnumerable<ValidationError> validationErrors)
    {
        return validationErrors
            .GroupBy(x => x.Identifier)
            .ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray());
    }
}
