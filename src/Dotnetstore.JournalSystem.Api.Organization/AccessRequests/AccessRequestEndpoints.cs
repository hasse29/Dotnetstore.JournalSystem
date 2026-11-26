using System.Security.Claims;
using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Authentication;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Organization.AccessRequests;

internal static class AccessRequestEndpoints
{
    internal static IEndpointRouteBuilder MapAccessRequestEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Signed-in Entra users who have not been approved yet have no role, so these only require authentication.
        var mine = app
            .MapGroup(string.Empty)
            .WithTags("Access requests")
            .RequireAuthorization(AuthPolicies.Authenticated);

        mine
            .MapPost(ApiEndpoints.Organization.AccessRequests.Me, RequestMyAccessAsync)
            .WithName("RequestMyAccess")
            .WithSummary("Records an access request for the signed-in Entra user, or returns the existing one.");

        mine
            .MapGet(ApiEndpoints.Organization.AccessRequests.Me, GetMineAsync)
            .WithName("GetMyAccessRequest")
            .WithSummary("Gets the signed-in Entra user's access request.");

        var admin = app
            .MapGroup(string.Empty)
            .WithTags("Access requests");

        admin
            .MapGet(ApiEndpoints.Organization.AccessRequests.GetAll, GetAllAsync)
            .WithName("GetAccessRequests")
            .RequireAuthorization(Permissions.AccessRequests.View)
            .WithSummary("Gets access requests, optionally filtered by status.");

        admin
            .MapPost(ApiEndpoints.Organization.AccessRequests.Approve, ApproveAsync)
            .WithName("ApproveAccessRequest")
            .RequireAuthorization(Permissions.AccessRequests.Edit)
            .WithSummary("Approves a pending request and creates the Entra user with the chosen role and company.");

        admin
            .MapPost(ApiEndpoints.Organization.AccessRequests.Reject, RejectAsync)
            .WithName("RejectAccessRequest")
            .RequireAuthorization(Permissions.AccessRequests.Edit)
            .WithSummary("Rejects a pending request.");

        admin
            .MapPost(ApiEndpoints.Organization.AccessRequests.Reopen, ReopenAsync)
            .WithName("ReopenAccessRequest")
            .RequireAuthorization(Permissions.AccessRequests.Edit)
            .WithSummary("Moves a rejected request back to pending.");

        return app;
    }

    private static async Task<Results<Ok<AccessRequestResponse>, ForbidHttpResult, ProblemHttpResult>> RequestMyAccessAsync(
        ClaimsPrincipal principal,
        IAccessRequestService service,
        CancellationToken cancellationToken)
    {
        // Identity comes from the validated token, never from the request body.
        var identity = EntraIdentity.From(principal);
        if (identity is null)
        {
            return TypedResults.Forbid();
        }

        var result = await service
            .RequestAccessAsync(identity, cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(result.Value),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Results<Ok<AccessRequestResponse>, NotFound, ForbidHttpResult>> GetMineAsync(
        ClaimsPrincipal principal,
        IAccessRequestService service,
        CancellationToken cancellationToken)
    {
        var identity = EntraIdentity.From(principal);
        if (identity is null)
        {
            return TypedResults.Forbid();
        }

        var result = await service
            .GetMineAsync(identity.ExternalId, cancellationToken)
            .ConfigureAwait(false);

        return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.NotFound();
    }

    private static async Task<Ok<AccessRequestResponse[]>> GetAllAsync(
        AccessRequestStatus? status,
        IAccessRequestService service,
        CancellationToken cancellationToken)
    {
        var items = await service
            .GetAllAsync(status, cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(items.ToArray());
    }

    private static async Task<Results<Ok<AccessRequestResponse>, NotFound, ValidationProblem, Conflict<string>, ProblemHttpResult>> ApproveAsync(
        Guid id,
        ApproveAccessRequestRequest? request,
        ClaimsPrincipal principal,
        IAccessRequestService service,
        IUserAdministrationGuard guard,
        CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var refusal = await guard
            .CheckAssignmentAsync(ActingUser.From(principal), request!.RoleId, cancellationToken)
            .ConfigureAwait(false);
        if (refusal is not null)
        {
            return TypedResults.Problem(refusal, statusCode: StatusCodes.Status403Forbidden);
        }

        var result = await service
            .ApproveAsync(id, request!, principal.GetUserId(), cancellationToken)
            .ConfigureAwait(false);

        return ToHttpResult(result);
    }

    private static async Task<Results<Ok<AccessRequestResponse>, NotFound, ValidationProblem, Conflict<string>, ProblemHttpResult>> RejectAsync(
        Guid id,
        RejectAccessRequestRequest? request,
        ClaimsPrincipal principal,
        IAccessRequestService service,
        CancellationToken cancellationToken)
    {
        var result = await service
            .RejectAsync(id, request ?? new RejectAccessRequestRequest(null), principal.GetUserId(), cancellationToken)
            .ConfigureAwait(false);

        return ToHttpResult(result);
    }

    private static async Task<Results<Ok<AccessRequestResponse>, NotFound, ValidationProblem, Conflict<string>, ProblemHttpResult>> ReopenAsync(
        Guid id,
        IAccessRequestService service,
        CancellationToken cancellationToken)
    {
        var result = await service
            .ReopenAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return ToHttpResult(result);
    }

    internal static Dictionary<string, string[]> Validate(ApproveAccessRequestRequest? request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request is null)
        {
            errors[nameof(request)] = ["A request body is required."];
            return errors;
        }

        if (request.OwnCompanyId == Guid.Empty)
        {
            errors[nameof(request.OwnCompanyId)] = ["An own company is required."];
        }

        if (request.RoleId == Guid.Empty)
        {
            errors[nameof(request.RoleId)] = ["A role is required."];
        }

        UserRequestValidator.ValidateNames(errors, request.FirstName, request.MiddleName, request.LastName);
        PersonDetailsValidator.Validate(errors, request.DateOfBirth, request.SocialSecurityNumber);

        return errors;
    }

    private static Results<Ok<AccessRequestResponse>, NotFound, ValidationProblem, Conflict<string>, ProblemHttpResult> ToHttpResult(
        Result<AccessRequestResponse> result)
    {
        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(result.Value),
            ResultStatus.NotFound => TypedResults.NotFound(),
            ResultStatus.Invalid => TypedResults.ValidationProblem(ToErrors(result.ValidationErrors)),
            ResultStatus.Conflict => TypedResults.Conflict(result.Errors.FirstOrDefault() ?? "Conflict."),
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
