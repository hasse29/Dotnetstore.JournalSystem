using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Organization.WorkGroups;

internal static class WorkGroupEndpoints
{
    private const string GetByIdName = "GetWorkGroupById";

    internal static IEndpointRouteBuilder MapWorkGroupEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app
            .MapGroup(string.Empty)
            .WithTags("Work groups");

        group
            .MapGet(ApiEndpoints.Organization.WorkGroups.GetAll, GetAllAsync)
            .WithName("GetWorkGroups")
            .WithSummary("Gets all work groups.");

        group
            .MapGet(ApiEndpoints.Organization.WorkGroups.GetById, GetByIdAsync)
            .WithName(GetByIdName)
            .WithSummary("Gets a work group by id.");

        group
            .MapPost(ApiEndpoints.Organization.WorkGroups.Create, CreateAsync)
            .WithName("CreateWorkGroup")
            .WithSummary("Creates a work group.");

        group
            .MapPut(ApiEndpoints.Organization.WorkGroups.Update, UpdateAsync)
            .WithName("UpdateWorkGroup")
            .WithSummary("Updates a work group.");

        group
            .MapDelete(ApiEndpoints.Organization.WorkGroups.Delete, DeleteAsync)
            .WithName("DeleteWorkGroup")
            .WithSummary("Deletes a work group.");

        return app;
    }

    private static async Task<Ok<WorkGroupResponse[]>> GetAllAsync(
        IWorkGroupService service,
        CancellationToken cancellationToken)
    {
        var items = await service
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(items.ToArray());
    }

    private static async Task<Results<Ok<WorkGroupResponse>, NotFound, ProblemHttpResult>> GetByIdAsync(
        Guid id,
        IWorkGroupService service,
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

    private static async Task<Results<CreatedAtRoute<WorkGroupResponse>, ValidationProblem, ProblemHttpResult>> CreateAsync(
        EditWorkGroupRequest? request,
        IWorkGroupService service,
        CancellationToken cancellationToken)
    {
        var errors = EditWorkGroupRequestValidator.Validate(request);
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

    private static async Task<Results<Ok<WorkGroupResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateAsync(
        Guid id,
        EditWorkGroupRequest? request,
        IWorkGroupService service,
        CancellationToken cancellationToken)
    {
        var errors = EditWorkGroupRequestValidator.Validate(request);
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
        IWorkGroupService service,
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
