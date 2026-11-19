using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;

internal static class PhoneNumberTypeEndpoints
{
    private const string GetByIdName = "GetPhoneNumberTypeById";

    internal static IEndpointRouteBuilder MapPhoneNumberTypeEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app
            .MapGroup(string.Empty)
            .WithTags("Phone number types");

        group
            .MapGet(ApiEndpoints.Contact.PhoneNumberTypes.GetAll, GetAllAsync)
            .WithName("GetPhoneNumberTypes")
            .WithSummary("Gets all phone number types.");

        group
            .MapGet(ApiEndpoints.Contact.PhoneNumberTypes.GetById, GetByIdAsync)
            .WithName(GetByIdName)
            .WithSummary("Gets an phone number type by id.");

        group
            .MapPost(ApiEndpoints.Contact.PhoneNumberTypes.Create, CreateAsync)
            .WithName("CreatePhoneNumberType")
            .WithSummary("Creates an phone number type.");

        group
            .MapPut(ApiEndpoints.Contact.PhoneNumberTypes.Update, UpdateAsync)
            .WithName("UpdatePhoneNumberType")
            .WithSummary("Updates an phone number type.");

        group
            .MapDelete(ApiEndpoints.Contact.PhoneNumberTypes.Delete, DeleteAsync)
            .WithName("DeletePhoneNumberType")
            .WithSummary("Deletes an phone number type.");

        return app;
    }

    private static async Task<Ok<PhoneNumberTypeResponse[]>> GetAllAsync(
        IPhoneNumberTypeService service,
        CancellationToken cancellationToken)
    {
        var items = await service
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(items.ToArray());
    }

    private static async Task<Results<Ok<PhoneNumberTypeResponse>, NotFound, ProblemHttpResult>> GetByIdAsync(
        Guid id,
        IPhoneNumberTypeService service,
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

    private static async Task<Results<CreatedAtRoute<PhoneNumberTypeResponse>, ValidationProblem, ProblemHttpResult>> CreateAsync(
        EditPhoneNumberTypeRequest? request,
        IPhoneNumberTypeService service,
        CancellationToken cancellationToken)
    {
        var errors = EditPhoneNumberTypeRequestValidator.Validate(request);
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

    private static async Task<Results<Ok<PhoneNumberTypeResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateAsync(
        Guid id,
        EditPhoneNumberTypeRequest? request,
        IPhoneNumberTypeService service,
        CancellationToken cancellationToken)
    {
        var errors = EditPhoneNumberTypeRequestValidator.Validate(request);
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
        IPhoneNumberTypeService service,
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
