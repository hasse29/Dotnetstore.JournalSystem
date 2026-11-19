using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dotnetstore.JournalSystem.Api.Organization.Services;

internal sealed class OrganizationUnitOfWork(
    OrganizationDataContext context,
    IOwnCompanyRepository ownCompanyRepository,
    IRoleRepository roleRepository) : IOrganizationUnitOfWork, IDisposable
{
    private IDbContextTransaction? _objTransaction;
    private bool _disposed;

    IOwnCompanyRepository IOrganizationUnitOfWork.OwnCompanies => ownCompanyRepository;
    IRoleRepository IOrganizationUnitOfWork.Roles => roleRepository;

    void IOrganizationUnitOfWork.CreateTransaction()
    {
        _objTransaction = context.Database.BeginTransaction();
    }

    async ValueTask IOrganizationUnitOfWork.CommitAsync(CancellationToken cancellationToken)
    {
        await GetTransaction()
            .CommitAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    async ValueTask IOrganizationUnitOfWork.RollbackAsync(CancellationToken cancellationToken)
    {
        await GetTransaction()
            .RollbackAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    async ValueTask<int> IOrganizationUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await context
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _objTransaction?.Dispose();
        context.Dispose();
        _disposed = true;
    }

    private IDbContextTransaction GetTransaction()
    {
        return _objTransaction
               ?? throw new InvalidOperationException("No transaction has been started. Call CreateTransaction first.");
    }
}
