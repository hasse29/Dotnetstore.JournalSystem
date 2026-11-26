using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal static class AddressEndpoints
{
    private const string GetByIdName = "GetAddressById";

    internal static IEndpointRouteBuilder MapAddressEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var reads = app
            .MapGroup(string.Empty)
            .WithTags("Addresses")
            .RequireAuthorization(Permissions.ContactDetails.View);

        reads
            .MapGet(ApiEndpoints.Contact.Addresses.GetAll, GetAllAsync)
            .WithName("GetAddresses")
            .WithSummary("Gets all addresses.");

        reads
            .MapGet(ApiEndpoints.Contact.Addresses.GetById, GetByIdAsync)
            .WithName(GetByIdName)
            .WithSummary("Gets an address by id.");

        var group = app
            .MapGroup(string.Empty)
            .WithTags("Addresses");

        group
            .MapPost(ApiEndpoints.Contact.Addresses.Create, CreateAsync)
            .WithName("CreateAddress")
            .RequireAuthorization(Permissions.ContactDetails.Create)
            .WithSummary("Creates an address.");

        group
            .MapPut(ApiEndpoints.Contact.Addresses.Update, UpdateAsync)
            .WithName("UpdateAddress")
            .RequireAuthorization(Permissions.ContactDetails.Edit)
            .WithSummary("Updates an address.");

        group
            .MapDelete(ApiEndpoints.Contact.Addresses.Delete, DeleteAsync)
            .WithName("DeleteAddress")
            .RequireAuthorization(Permissions.ContactDetails.Delete)
            .WithSummary("Deletes an address.");

        return app;
    }

    private static async Task<Ok<AddressResponse[]>> GetAllAsync(
        Guid? objectId,
        IAddressService service,
        CancellationToken cancellationToken)
    {
        var items = objectId is { } id
            ? await service.GetByObjectIdAsync(id, cancellationToken).ConfigureAwait(false)
            : await service.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(items.ToArray());
    }

    private static async Task<Results<Ok<AddressResponse>, NotFound, ProblemHttpResult>> GetByIdAsync(
        Guid id,
        IAddressService service,
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

    private static async Task<Results<CreatedAtRoute<AddressResponse>, ValidationProblem, ProblemHttpResult>> CreateAsync(
        EditAddressRequest? request,
        IAddressService service,
        CancellationToken cancellationToken)
    {
        var errors = EditAddressRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var result = await service
            .CreateAsync(request!, null, cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.CreatedAtRoute(result.Value, GetByIdName, new RouteValueDictionary { ["id"] = result.Value.Id }),
            ResultStatus.Invalid => TypedResults.ValidationProblem(ToErrors(result.ValidationErrors)),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Results<Ok<AddressResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateAsync(
        Guid id,
        EditAddressRequest? request,
        IAddressService service,
        CancellationToken cancellationToken)
    {
        var errors = EditAddressRequestValidator.Validate(request);
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
            ResultStatus.Invalid => TypedResults.ValidationProblem(ToErrors(result.ValidationErrors)),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteAsync(
        Guid id,
        IAddressService service,
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

    private static Dictionary<string, string[]> ToErrors(IEnumerable<ValidationError> validationErrors)
    {
        return validationErrors
            .GroupBy(x => x.Identifier ?? string.Empty)
            .ToDictionary(x => x.Key, x => x.Select(e => e.ErrorMessage).ToArray());
    }
}
