using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Contact.AddressTypes;

internal static class AddressTypeEndpoints
{
    private const string GetByIdName = "GetAddressTypeById";

    internal static IEndpointRouteBuilder MapAddressTypeEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var reads = app
            .MapGroup(string.Empty)
            .WithTags("Address types")
            .RequireAuthorization(AuthPolicies.Authenticated);

        reads
            .MapGet(ApiEndpoints.Contact.AddressTypes.GetAll, GetAllAsync)
            .WithName("GetAddressTypes")
            .WithSummary("Gets all address types.");

        reads
            .MapGet(ApiEndpoints.Contact.AddressTypes.GetById, GetByIdAsync)
            .WithName(GetByIdName)
            .WithSummary("Gets an address type by id.");

        var group = app
            .MapGroup(string.Empty)
            .WithTags("Address types")
            .RequireAuthorization(AuthPolicies.Administrator);

        group
            .MapPost(ApiEndpoints.Contact.AddressTypes.Create, CreateAsync)
            .WithName("CreateAddressType")
            .WithSummary("Creates an address type.");

        group
            .MapPut(ApiEndpoints.Contact.AddressTypes.Update, UpdateAsync)
            .WithName("UpdateAddressType")
            .WithSummary("Updates an address type.");

        group
            .MapDelete(ApiEndpoints.Contact.AddressTypes.Delete, DeleteAsync)
            .WithName("DeleteAddressType")
            .WithSummary("Deletes an address type.");

        return app;
    }

    private static async Task<Ok<AddressTypeResponse[]>> GetAllAsync(
        IAddressTypeService service,
        CancellationToken cancellationToken)
    {
        var items = await service
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(items.ToArray());
    }

    private static async Task<Results<Ok<AddressTypeResponse>, NotFound, ProblemHttpResult>> GetByIdAsync(
        Guid id,
        IAddressTypeService service,
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

    private static async Task<Results<CreatedAtRoute<AddressTypeResponse>, ValidationProblem, ProblemHttpResult>> CreateAsync(
        EditAddressTypeRequest? request,
        IAddressTypeService service,
        CancellationToken cancellationToken)
    {
        var errors = EditAddressTypeRequestValidator.Validate(request);
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

    private static async Task<Results<Ok<AddressTypeResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateAsync(
        Guid id,
        EditAddressTypeRequest? request,
        IAddressTypeService service,
        CancellationToken cancellationToken)
    {
        var errors = EditAddressTypeRequestValidator.Validate(request);
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
        IAddressTypeService service,
        CancellationToken cancellationToken)
    {
        var result = await service
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.NoContent(),
            ResultStatus.Conflict => TypedResults.Problem(detail: string.Join(" ", result.Errors), statusCode: StatusCodes.Status409Conflict),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem()
        };
    }
}
