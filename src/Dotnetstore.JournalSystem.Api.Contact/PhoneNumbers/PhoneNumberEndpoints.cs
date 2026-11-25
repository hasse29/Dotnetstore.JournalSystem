using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal static class PhoneNumberEndpoints
{
    private const string GetByIdName = "GetPhoneNumberById";

    internal static IEndpointRouteBuilder MapPhoneNumberEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var reads = app
            .MapGroup(string.Empty)
            .WithTags("Phone numbers")
            .RequireAuthorization(AuthPolicies.Authenticated);

        reads
            .MapGet(ApiEndpoints.Contact.PhoneNumbers.GetAll, GetAllAsync)
            .WithName("GetPhoneNumbers")
            .WithSummary("Gets all phone numbers.");

        reads
            .MapGet(ApiEndpoints.Contact.PhoneNumbers.GetById, GetByIdAsync)
            .WithName(GetByIdName)
            .WithSummary("Gets a phone number by id.");

        var group = app
            .MapGroup(string.Empty)
            .WithTags("Phone numbers")
            .RequireAuthorization(AuthPolicies.Administrator);

        group
            .MapPost(ApiEndpoints.Contact.PhoneNumbers.Create, CreateAsync)
            .WithName("CreatePhoneNumber")
            .WithSummary("Creates a phone number.");

        group
            .MapPut(ApiEndpoints.Contact.PhoneNumbers.Update, UpdateAsync)
            .WithName("UpdatePhoneNumber")
            .WithSummary("Updates a phone number.");

        group
            .MapDelete(ApiEndpoints.Contact.PhoneNumbers.Delete, DeleteAsync)
            .WithName("DeletePhoneNumber")
            .WithSummary("Deletes a phone number.");

        return app;
    }

    private static async Task<Ok<PhoneNumberResponse[]>> GetAllAsync(
        Guid? objectId,
        IPhoneNumberService service,
        CancellationToken cancellationToken)
    {
        var items = objectId is { } id
            ? await service.GetByObjectIdAsync(id, cancellationToken).ConfigureAwait(false)
            : await service.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(items.ToArray());
    }

    private static async Task<Results<Ok<PhoneNumberResponse>, NotFound, ProblemHttpResult>> GetByIdAsync(
        Guid id,
        IPhoneNumberService service,
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

    private static async Task<Results<CreatedAtRoute<PhoneNumberResponse>, ValidationProblem, ProblemHttpResult>> CreateAsync(
        EditPhoneNumberRequest? request,
        IPhoneNumberService service,
        CancellationToken cancellationToken)
    {
        var errors = EditPhoneNumberRequestValidator.Validate(request);
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

    private static async Task<Results<Ok<PhoneNumberResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateAsync(
        Guid id,
        EditPhoneNumberRequest? request,
        IPhoneNumberService service,
        CancellationToken cancellationToken)
    {
        var errors = EditPhoneNumberRequestValidator.Validate(request);
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
        IPhoneNumberService service,
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
