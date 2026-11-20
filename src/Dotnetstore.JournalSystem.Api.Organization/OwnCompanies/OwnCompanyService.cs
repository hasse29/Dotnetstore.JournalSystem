using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal sealed class OwnCompanyService(
    IOrganizationUnitOfWork organizationUnitOfWork) : IOwnCompanyService
{
    async Task<IEnumerable<OwnCompanyResponse>> IOwnCompanyService.GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await organizationUnitOfWork
            .OwnCompanies
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.ToOwnCompanyResponse());
    }

    async ValueTask<Result<OwnCompanyResponse>> IOwnCompanyService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ownCompany = await organizationUnitOfWork
            .OwnCompanies
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return ownCompany is not null ?
            Result<OwnCompanyResponse>.Success(ownCompany.ToOwnCompanyResponse()) :
            Result<OwnCompanyResponse>.NotFound();
    }

    async ValueTask<Result<OwnCompanyResponse>> IOwnCompanyService.CreateAsync(EditOwnCompanyRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        var ownCompany = OwnCompanyBuilder
            .Create()
            .WithCreateValues(request, userId)
            .Build();

        await organizationUnitOfWork
            .OwnCompanies
            .AddAsync(ownCompany, cancellationToken)
            .ConfigureAwait(false);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<OwnCompanyResponse>.Success(ownCompany.ToOwnCompanyResponse()) :
            Result<OwnCompanyResponse>.Error("Failed to create the own company.");
    }

    async ValueTask<Result<OwnCompanyResponse>> IOwnCompanyService.UpdateAsync(Guid id, EditOwnCompanyRequest ownCompany, Guid? userId, CancellationToken cancellationToken)
    {
        var existing = await organizationUnitOfWork
            .OwnCompanies
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result<OwnCompanyResponse>.NotFound();
        }

        var ownCompanyToUpdate = OwnCompanyBuilder
            .Update()
            .WithUpdateValues(existing, ownCompany, userId)
            .Build();

        organizationUnitOfWork
            .OwnCompanies
            .Detach(existing);

        organizationUnitOfWork
            .OwnCompanies
            .Update(ownCompanyToUpdate);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<OwnCompanyResponse>.Success(ownCompanyToUpdate.ToOwnCompanyResponse()) :
            Result<OwnCompanyResponse>.Error("Failed to update the own company.");
    }

    async ValueTask<Result> IOwnCompanyService.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await organizationUnitOfWork
            .OwnCompanies
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.NotFound();
        }

        await organizationUnitOfWork
            .OwnCompanies
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result.Success() :
            Result.Error("Failed to delete the own company.");
    }
}