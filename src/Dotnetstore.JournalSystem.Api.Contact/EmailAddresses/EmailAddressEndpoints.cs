using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal static class EmailAddressEndpoints
{
    private const string GetByIdName = "GetEmailAddressById";

    internal static IEndpointRouteBuilder MapEmailAddressEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var reads = app
            .MapGroup(string.Empty)
            .WithTags("Email addresses")
            .RequireAuthorization(AuthPolicies.Approved);

        reads
            .MapGet(ApiEndpoints.Contact.EmailAddresses.GetAll, GetAllAsync)
            .WithName("GetEmailAddresses")
            .WithSummary("Gets all email addresses.");

        reads
            .MapGet(ApiEndpoints.Contact.EmailAddresses.GetById, GetByIdAsync)
            .WithName(GetByIdName)
            .WithSummary("Gets an email address by id.");

        var group = app
            .MapGroup(string.Empty)
            .WithTags("Email addresses")
            .RequireAuthorization(AuthPolicies.Administrator);

        group
            .MapPost(ApiEndpoints.Contact.EmailAddresses.Create, CreateAsync)
            .WithName("CreateEmailAddress")
            .WithSummary("Creates an email address.");

        group
            .MapPut(ApiEndpoints.Contact.EmailAddresses.Update, UpdateAsync)
            .WithName("UpdateEmailAddress")
            .WithSummary("Updates an email address.");

        group
            .MapDelete(ApiEndpoints.Contact.EmailAddresses.Delete, DeleteAsync)
            .WithName("DeleteEmailAddress")
            .WithSummary("Deletes an email address.");

        return app;
    }

    private static async Task<Ok<EmailAddressResponse[]>> GetAllAsync(
        Guid? objectId,
        IEmailAddressService service,
        CancellationToken cancellationToken)
    {
        var items = objectId is { } id
            ? await service.GetByObjectIdAsync(id, cancellationToken).ConfigureAwait(false)
            : await service.GetAllAsync(cancellationToken).ConfigureAwait(false);

        return TypedResults.Ok(items.ToArray());
    }

    private static async Task<Results<Ok<EmailAddressResponse>, NotFound, ProblemHttpResult>> GetByIdAsync(
        Guid id,
        IEmailAddressService service,
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

    private static async Task<Results<CreatedAtRoute<EmailAddressResponse>, ValidationProblem, ProblemHttpResult>> CreateAsync(
        EditEmailAddressRequest? request,
        IEmailAddressService service,
        CancellationToken cancellationToken)
    {
        var errors = EditEmailAddressRequestValidator.Validate(request);
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

    private static async Task<Results<Ok<EmailAddressResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateAsync(
        Guid id,
        EditEmailAddressRequest? request,
        IEmailAddressService service,
        CancellationToken cancellationToken)
    {
        var errors = EditEmailAddressRequestValidator.Validate(request);
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
        IEmailAddressService service,
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
