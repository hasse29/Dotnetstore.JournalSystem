using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;

internal sealed class OwnCompanyImageService(
    IOrganizationUnitOfWork organizationUnitOfWork,
    IOwnCompanyImageRepository images,
    TimeProvider clock) : IOwnCompanyImageService
{
    async Task<Result<IReadOnlyList<OwnCompanyImageResponse>>> IOwnCompanyImageService.GetAllAsync(Guid ownCompanyId, CancellationToken cancellationToken)
    {
        if (!await CompanyExistsAsync(ownCompanyId, cancellationToken).ConfigureAwait(false))
        {
            return Result<IReadOnlyList<OwnCompanyImageResponse>>.NotFound();
        }

        var items = await images
            .ListAsync(ownCompanyId, cancellationToken)
            .ConfigureAwait(false);

        return Result<IReadOnlyList<OwnCompanyImageResponse>>.Success(items);
    }

    async Task<Result<OwnCompanyImage>> IOwnCompanyImageService.GetAsync(Guid ownCompanyId, Guid imageId, CancellationToken cancellationToken)
    {
        var image = await images
            .FindAsync(ownCompanyId, imageId, cancellationToken)
            .ConfigureAwait(false);

        return image is not null
            ? Result<OwnCompanyImage>.Success(image)
            : Result<OwnCompanyImage>.NotFound();
    }

    async Task<Result<OwnCompanyImageResponse>> IOwnCompanyImageService.SetLogoAsync(
        Guid ownCompanyId,
        string? fileName,
        byte[] content,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        if (!await CompanyExistsAsync(ownCompanyId, cancellationToken).ConfigureAwait(false))
        {
            return Result<OwnCompanyImageResponse>.NotFound();
        }

        var existing = await images
            .FindLogoAsync(ownCompanyId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            await images
                .DeleteAsync(existing.Id, cancellationToken)
                .ConfigureAwait(false);
        }

        // A new id for every logo, so browsers never show a cached, replaced one.
        var logo = CreateImage(Guid.CreateVersion7(), ownCompanyId, isLogo: true, fileName, content, userId);

        return await SaveAsync(logo, cancellationToken).ConfigureAwait(false);
    }

    async Task<Result<OwnCompanyImageResponse>> IOwnCompanyImageService.AddPhotoAsync(
        Guid ownCompanyId,
        Guid imageId,
        string? fileName,
        byte[] content,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        if (!await CompanyExistsAsync(ownCompanyId, cancellationToken).ConfigureAwait(false))
        {
            return Result<OwnCompanyImageResponse>.NotFound();
        }

        var existing = await images
            .GetByIdAsync(imageId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            return existing.OwnCompanyId == ownCompanyId && !existing.IsLogo
                ? Result<OwnCompanyImageResponse>.Success(ToResponse(existing))
                : Result<OwnCompanyImageResponse>.Conflict("The image id is already in use.");
        }

        var count = await images
            .CountPhotosAsync(ownCompanyId, cancellationToken)
            .ConfigureAwait(false);

        if (count >= OwnCompanyImageConstants.MaxPhotosPerCompany)
        {
            return Result<OwnCompanyImageResponse>.Conflict(
                $"A company can have at most {OwnCompanyImageConstants.MaxPhotosPerCompany} photos.");
        }

        var photo = CreateImage(imageId, ownCompanyId, isLogo: false, fileName, content, userId);

        return await SaveAsync(photo, cancellationToken).ConfigureAwait(false);
    }

    async Task<Result> IOwnCompanyImageService.DeleteAsync(Guid ownCompanyId, Guid imageId, CancellationToken cancellationToken)
    {
        var existing = await images
            .FindAsync(ownCompanyId, imageId, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.NotFound();
        }

        await images
            .DeleteAsync(imageId, cancellationToken)
            .ConfigureAwait(false);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result.Success() :
            Result.Error("Failed to delete the image.");
    }

    private async Task<bool> CompanyExistsAsync(Guid ownCompanyId, CancellationToken cancellationToken)
    {
        var company = await organizationUnitOfWork
            .OwnCompanies
            .GetByIdAsync(ownCompanyId, cancellationToken)
            .ConfigureAwait(false);

        return company is not null;
    }

    private OwnCompanyImage CreateImage(Guid id, Guid ownCompanyId, bool isLogo, string? fileName, byte[] content, Guid? userId)
    {
        return new OwnCompanyImage
        {
            Id = id,
            OwnCompanyId = ownCompanyId,
            IsLogo = isLogo,
            FileName = CleanFileName(fileName),
            ContentType = OwnCompanyImageValidator.DetectContentType(content)
                ?? throw new ArgumentException("The content is not a supported image.", nameof(content)),
            SizeBytes = content.Length,
            Content = content,
            CreatedDate = clock.GetUtcNow().UtcDateTime,
            CreatedBy = userId
        };
    }

    private async Task<Result<OwnCompanyImageResponse>> SaveAsync(OwnCompanyImage image, CancellationToken cancellationToken)
    {
        await images
            .AddAsync(image, cancellationToken)
            .ConfigureAwait(false);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<OwnCompanyImageResponse>.Success(ToResponse(image)) :
            Result<OwnCompanyImageResponse>.Error("Failed to save the image.");
    }

    private static string CleanFileName(string? fileName)
    {
        // Not Path.GetFileName: on Linux it keeps a Windows client's directories, as '\' is no separator there.
        var path = fileName?.Trim() ?? string.Empty;
        var name = path[(path.LastIndexOfAny(['/', '\\']) + 1)..];

        if (string.IsNullOrEmpty(name))
        {
            return "image";
        }

        return name.Length <= OwnCompanyImageConstants.FileNameMaxLength
            ? name
            : name[^OwnCompanyImageConstants.FileNameMaxLength..];
    }

    private static OwnCompanyImageResponse ToResponse(OwnCompanyImage image) =>
        new(image.Id, image.IsLogo, image.FileName, image.ContentType, image.SizeBytes, image.CreatedDate);
}
