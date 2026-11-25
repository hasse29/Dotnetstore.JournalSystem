using System.Security.Claims;
using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Authentication;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;

internal static class OwnCompanyImageEndpoints
{
    internal static IEndpointRouteBuilder MapOwnCompanyImageEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var reads = app
            .MapGroup(string.Empty)
            .WithTags("Own company images")
            .RequireAuthorization(AuthPolicies.Approved);

        reads
            .MapGet(ApiEndpoints.Organization.OwnCompanies.Images, GetAllAsync)
            .WithName("GetOwnCompanyImages")
            .WithSummary("Lists the logo and photos of an own company.");

        reads
            .MapGet(ApiEndpoints.Organization.OwnCompanies.ImageById, GetContentAsync)
            .WithName("GetOwnCompanyImage")
            .WithSummary("Downloads an image of an own company.");

        var group = app
            .MapGroup(string.Empty)
            .WithTags("Own company images")
            .RequireAuthorization(AuthPolicies.Administrator);

        // Antiforgery protects cookie authentication; this API only accepts bearer tokens.
        group
            .MapPut(ApiEndpoints.Organization.OwnCompanies.Logo, SetLogoAsync)
            .WithName("SetOwnCompanyLogo")
            .WithSummary("Uploads or replaces the logo of an own company.")
            .WithMetadata(new RequestSizeLimitAttribute(OwnCompanyImageConstants.MaxRequestBytes))
            .DisableAntiforgery();

        group
            .MapPut(ApiEndpoints.Organization.OwnCompanies.Photo, AddPhotoAsync)
            .WithName("AddOwnCompanyPhoto")
            .WithSummary("Adds a photo to an own company, under an id chosen by the caller.")
            .WithMetadata(new RequestSizeLimitAttribute(OwnCompanyImageConstants.MaxRequestBytes))
            .DisableAntiforgery();

        group
            .MapDelete(ApiEndpoints.Organization.OwnCompanies.ImageById, DeleteAsync)
            .WithName("DeleteOwnCompanyImage")
            .WithSummary("Deletes a logo or photo of an own company.");

        return app;
    }

    private static async Task<Results<Ok<OwnCompanyImageResponse[]>, NotFound, ProblemHttpResult>> GetAllAsync(
        Guid id,
        IOwnCompanyImageService service,
        CancellationToken cancellationToken)
    {
        var result = await service
            .GetAllAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(result.Value.ToArray()),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Results<FileContentHttpResult, NotFound, ProblemHttpResult>> GetContentAsync(
        Guid id,
        Guid imageId,
        IOwnCompanyImageService service,
        CancellationToken cancellationToken)
    {
        var result = await service
            .GetAsync(id, imageId, cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.File(result.Value.Content, result.Value.ContentType),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Results<Ok<OwnCompanyImageResponse>, NotFound, ValidationProblem, ProblemHttpResult>> SetLogoAsync(
        Guid id,
        IFormFile? file,
        ClaimsPrincipal principal,
        IOwnCompanyImageService service,
        CancellationToken cancellationToken)
    {
        var content = await ReadAsync(file, cancellationToken).ConfigureAwait(false);
        var errors = OwnCompanyImageValidator.Validate(content);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var result = await service
            .SetLogoAsync(id, file!.FileName, content!, principal.GetUserId(), cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(result.Value),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Results<Ok<OwnCompanyImageResponse>, NotFound, Conflict<string>, ValidationProblem, ProblemHttpResult>> AddPhotoAsync(
        Guid id,
        Guid imageId,
        IFormFile? file,
        ClaimsPrincipal principal,
        IOwnCompanyImageService service,
        CancellationToken cancellationToken)
    {
        var content = await ReadAsync(file, cancellationToken).ConfigureAwait(false);
        var errors = OwnCompanyImageValidator.Validate(content);
        if (imageId == Guid.Empty)
        {
            errors[nameof(imageId)] = ["An image id is required."];
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var result = await service
            .AddPhotoAsync(id, imageId, file!.FileName, content!, principal.GetUserId(), cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.Ok(result.Value),
            ResultStatus.NotFound => TypedResults.NotFound(),
            ResultStatus.Conflict => TypedResults.Conflict(result.Errors.FirstOrDefault() ?? "Conflict."),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteAsync(
        Guid id,
        Guid imageId,
        IOwnCompanyImageService service,
        CancellationToken cancellationToken)
    {
        var result = await service
            .DeleteAsync(id, imageId, cancellationToken)
            .ConfigureAwait(false);

        return result.Status switch
        {
            ResultStatus.Ok => TypedResults.NoContent(),
            ResultStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem()
        };
    }

    private static async Task<byte[]?> ReadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return null;
        }

        // One byte past the limit is enough for the validator to reject it, without buffering the whole file.
        var buffer = new byte[Math.Min(file.Length, OwnCompanyImageConstants.MaxSizeBytes + 1L)];
        var stream = file.OpenReadStream();
        await using (stream.ConfigureAwait(false))
        {
            await stream.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        return buffer;
    }
}
