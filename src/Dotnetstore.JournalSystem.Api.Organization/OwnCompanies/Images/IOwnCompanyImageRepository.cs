using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;

internal interface IOwnCompanyImageRepository : IGenericRepository<OwnCompanyImage>
{
    /// <summary>Lists the images of a company without loading their content.</summary>
    Task<IReadOnlyList<OwnCompanyImageResponse>> ListAsync(Guid ownCompanyId, CancellationToken cancellationToken = default);

    Task<OwnCompanyImage?> FindAsync(Guid ownCompanyId, Guid imageId, CancellationToken cancellationToken = default);

    Task<OwnCompanyImage?> FindLogoAsync(Guid ownCompanyId, CancellationToken cancellationToken = default);

    Task<int> CountPhotosAsync(Guid ownCompanyId, CancellationToken cancellationToken = default);
}
