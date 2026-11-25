using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal static class RoleEndpoints
{
    private const string GetByIdName = "GetRoleById";

    internal static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var reads = app
            .MapGroup(string.Empty)
            .WithTags("Roles")
            .RequireAuthorization(AuthPolicies.Approved);

        reads
            .MapGet(ApiEndpoints.Organization.Roles.GetAll, GetAllAsync)
            .WithName("GetRoles")
            .WithSummary("Gets all roles.");

        reads
            .MapGet(ApiEndpoints.Organization.Roles.GetById, GetByIdAsync)
            .WithName(GetByIdName)
            .WithSummary("Gets a role by id.");

        var group = app
            .MapGroup(string.Empty)
            .WithTags("Roles")
            .RequireAuthorization(AuthPolicies.Administrator);

        group
            .MapPost(ApiEndpoints.Organization.Roles.Create, CreateAsync)
            .WithName("CreateRole")
            .WithSummary("Creates a role.");

        group
            .MapPut(ApiEndpoints.Organization.Roles.Update, UpdateAsync)
            .WithName("UpdateRole")
            .WithSummary("Updates a role.");

        group
            .MapDelete(ApiEndpoints.Organization.Roles.Delete, DeleteAsync)
            .WithName("DeleteRole")
            .WithSummary("Deletes a role.");

        return app;
    }

    private static async Task<Ok<RoleResponse[]>> GetAllAsync(
        IRoleService service,
        CancellationToken cancellationToken)
    {
        var items = await service
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(items.ToArray());
    }

    private static async Task<Results<Ok<RoleResponse>, NotFound, ProblemHttpResult>> GetByIdAsync(
        Guid id,
        IRoleService service,
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

    private static async Task<Results<CreatedAtRoute<RoleResponse>, ValidationProblem, ProblemHttpResult>> CreateAsync(
        EditRoleRequest? request,
        IRoleService service,
        CancellationToken cancellationToken)
    {
        var errors = EditRoleRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var result = await service
            .CreateAsync(request!, null, cancellationToken)
            .ConfigureAwait(false);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, GetByIdName, new RouteValueDictionary { ["id"] = result.Value.Id })
            : TypedResults.Problem();
    }

    private static async Task<Results<Ok<RoleResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateAsync(
        Guid id,
        EditRoleRequest? request,
        IRoleService service,
        CancellationToken cancellationToken)
    {
        var errors = EditRoleRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var result = await service
            .UpdateAsync(id, request!, null, cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(result.Value),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteAsync(
        Guid id,
        IRoleService service,
        CancellationToken cancellationToken)
    {
        var result = await service
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.NoContent(),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem()
        };
    }
}
