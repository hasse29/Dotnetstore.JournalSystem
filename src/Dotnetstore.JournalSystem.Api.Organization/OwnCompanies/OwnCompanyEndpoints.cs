using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal static class OwnCompanyEndpoints
{
    private const string GetByIdName = "GetOwnCompanyById";

    internal static IEndpointRouteBuilder MapOwnCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app
            .MapGroup(string.Empty)
            .WithTags("Own companies");

        group
            .MapGet(ApiEndpoints.Organization.OwnCompanies.GetAll, GetAllAsync)
            .WithName("GetOwnCompanies")
            .WithSummary("Gets all own companies.");

        group
            .MapGet(ApiEndpoints.Organization.OwnCompanies.GetById, GetByIdAsync)
            .WithName(GetByIdName)
            .WithSummary("Gets an own company by id.");

        group
            .MapPost(ApiEndpoints.Organization.OwnCompanies.Create, CreateAsync)
            .WithName("CreateOwnCompany")
            .WithSummary("Creates an own company.");

        group
            .MapPut(ApiEndpoints.Organization.OwnCompanies.Update, UpdateAsync)
            .WithName("UpdateOwnCompany")
            .WithSummary("Updates an own company.");

        group
            .MapDelete(ApiEndpoints.Organization.OwnCompanies.Delete, DeleteAsync)
            .WithName("DeleteOwnCompany")
            .WithSummary("Deletes an own company.");

        return app;
    }

    private static async Task<Ok<OwnCompanyResponse[]>> GetAllAsync(
        IOwnCompanyService service,
        CancellationToken cancellationToken)
    {
        var items = await service
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(items.ToArray());
    }

    private static async Task<Results<Ok<OwnCompanyResponse>, NotFound, ProblemHttpResult>> GetByIdAsync(
        Guid id,
        IOwnCompanyService service,
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

    private static async Task<Results<CreatedAtRoute<OwnCompanyResponse>, ValidationProblem, ProblemHttpResult>> CreateAsync(
        EditOwnCompanyRequest? request,
        IOwnCompanyService service,
        CancellationToken cancellationToken)
    {
        var errors = EditOwnCompanyRequestValidator.Validate(request);
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

    private static async Task<Results<Ok<OwnCompanyResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateAsync(
        Guid id,
        EditOwnCompanyRequest? request,
        IOwnCompanyService service,
        CancellationToken cancellationToken)
    {
        var errors = EditOwnCompanyRequestValidator.Validate(request);
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
        IOwnCompanyService service,
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
