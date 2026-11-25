using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;

internal sealed class OwnCompanyImageRepository(OrganizationDataContext dataContext)
    : GenericRepository<OwnCompanyImage>(dataContext), IOwnCompanyImageRepository
{
    async Task<IReadOnlyList<OwnCompanyImageResponse>> IOwnCompanyImageRepository.ListAsync(Guid ownCompanyId, CancellationToken cancellationToken)
    {
        return await dataContext
            .OwnCompanyImages
            .AsNoTracking()
            .Where(x => x.OwnCompanyId == ownCompanyId)
            .OrderBy(x => x.CreatedDate)
            .ThenBy(x => x.Id)
            .Select(x => new OwnCompanyImageResponse(x.Id, x.IsLogo, x.FileName, x.ContentType, x.SizeBytes, x.CreatedDate))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    async Task<OwnCompanyImage?> IOwnCompanyImageRepository.FindAsync(Guid ownCompanyId, Guid imageId, CancellationToken cancellationToken)
    {
        return await dataContext
            .OwnCompanyImages
            .FirstOrDefaultAsync(x => x.Id == imageId && x.OwnCompanyId == ownCompanyId, cancellationToken)
            .ConfigureAwait(false);
    }

    async Task<OwnCompanyImage?> IOwnCompanyImageRepository.FindLogoAsync(Guid ownCompanyId, CancellationToken cancellationToken)
    {
        return await dataContext
            .OwnCompanyImages
            .FirstOrDefaultAsync(x => x.OwnCompanyId == ownCompanyId && x.IsLogo, cancellationToken)
            .ConfigureAwait(false);
    }

    async Task<int> IOwnCompanyImageRepository.CountPhotosAsync(Guid ownCompanyId, CancellationToken cancellationToken)
    {
        return await dataContext
            .OwnCompanyImages
            .CountAsync(x => x.OwnCompanyId == ownCompanyId && !x.IsLogo, cancellationToken)
            .ConfigureAwait(false);
    }
}
