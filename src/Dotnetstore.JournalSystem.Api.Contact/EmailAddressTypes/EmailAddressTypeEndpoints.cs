using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;

internal static class EmailAddressTypeEndpoints
{
    private const string GetByIdName = "GetEmailAddressTypeById";

    internal static IEndpointRouteBuilder MapEmailAddressTypeEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app
            .MapGroup(string.Empty)
            .WithTags("Email address types");

        group
            .MapGet(ApiEndpoints.Contact.EmailAddressTypes.GetAll, GetAllAsync)
            .WithName("GetEmailAddressTypes")
            .WithSummary("Gets all email address types.");

        group
            .MapGet(ApiEndpoints.Contact.EmailAddressTypes.GetById, GetByIdAsync)
            .WithName(GetByIdName)
            .WithSummary("Gets an email address type by id.");

        group
            .MapPost(ApiEndpoints.Contact.EmailAddressTypes.Create, CreateAsync)
            .WithName("CreateEmailAddressType")
            .WithSummary("Creates an email address type.");

        group
            .MapPut(ApiEndpoints.Contact.EmailAddressTypes.Update, UpdateAsync)
            .WithName("UpdateEmailAddressType")
            .WithSummary("Updates an email address type.");

        group
            .MapDelete(ApiEndpoints.Contact.EmailAddressTypes.Delete, DeleteAsync)
            .WithName("DeleteEmailAddressType")
            .WithSummary("Deletes an email address type.");

        return app;
    }

    private static async Task<Ok<EmailAddressTypeResponse[]>> GetAllAsync(
        IEmailAddressTypeService service,
        CancellationToken cancellationToken)
    {
        var items = await service
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return TypedResults.Ok(items.ToArray());
    }

    private static async Task<Results<Ok<EmailAddressTypeResponse>, NotFound, ProblemHttpResult>> GetByIdAsync(
        Guid id,
        IEmailAddressTypeService service,
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

    private static async Task<Results<CreatedAtRoute<EmailAddressTypeResponse>, ValidationProblem, ProblemHttpResult>> CreateAsync(
        EditEmailAddressTypeRequest? request,
        IEmailAddressTypeService service,
        CancellationToken cancellationToken)
    {
        var errors = EditEmailAddressTypeRequestValidator.Validate(request);
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

    private static async Task<Results<Ok<EmailAddressTypeResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateAsync(
        Guid id,
        EditEmailAddressTypeRequest? request,
        IEmailAddressTypeService service,
        CancellationToken cancellationToken)
    {
        var errors = EditEmailAddressTypeRequestValidator.Validate(request);
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
        IEmailAddressTypeService service,
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
