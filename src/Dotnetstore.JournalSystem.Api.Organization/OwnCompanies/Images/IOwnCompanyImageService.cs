using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;

internal interface IOwnCompanyImageService
{
    Task<Result<IReadOnlyList<OwnCompanyImageResponse>>> GetAllAsync(Guid ownCompanyId, CancellationToken cancellationToken = default);

    Task<Result<OwnCompanyImage>> GetAsync(Guid ownCompanyId, Guid imageId, CancellationToken cancellationToken = default);

    /// <summary>Stores a logo, replacing any logo the company already has. The content must already be validated.</summary>
    Task<Result<OwnCompanyImageResponse>> SetLogoAsync(
        Guid ownCompanyId,
        string? fileName,
        byte[] content,
        Guid? userId,
        CancellationToken cancellationToken = default);

    /// <summary>Stores a photo under a caller-chosen id, so a repeated request does not add it twice. The content must already be validated.</summary>
    Task<Result<OwnCompanyImageResponse>> AddPhotoAsync(
        Guid ownCompanyId,
        Guid imageId,
        string? fileName,
        byte[] content,
        Guid? userId,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid ownCompanyId, Guid imageId, CancellationToken cancellationToken = default);
}
