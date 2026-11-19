using Ardalis.Result;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal interface IOwnCompanyService
{
    Task<IEnumerable<OwnCompanyResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    ValueTask<Result<OwnCompanyResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    ValueTask<Result<OwnCompanyResponse>> CreateAsync(EditOwnCompanyRequest ownCompany, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result<OwnCompanyResponse>> UpdateAsync(Guid id, EditOwnCompanyRequest ownCompany, Guid? userId, CancellationToken cancellationToken = default);
    ValueTask<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}