using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dotnetstore.JournalSystem.Api.Contact.Services;

internal sealed class ContactUnitOfWork(
    ContactDataContext context,
    IAddressTypeRepository addressTypeRepository,
    IAddressRepository addressRepository,
    IEmailAddressTypeRepository emailAddressTypeRepository) : IContactUnitOfWork, IDisposable
{
    private IDbContextTransaction? _objTransaction;
    private bool _disposed;

    IAddressTypeRepository IContactUnitOfWork.AddressTypes => addressTypeRepository;
    IAddressRepository IContactUnitOfWork.Addresses => addressRepository;
    IEmailAddressTypeRepository IContactUnitOfWork.EmailAddressTypes => emailAddressTypeRepository;

    void IContactUnitOfWork.CreateTransaction()
    {
        _objTransaction = context.Database.BeginTransaction();
    }

    async ValueTask IContactUnitOfWork.CommitAsync(CancellationToken cancellationToken)
    {
        await GetTransaction()
            .CommitAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    async ValueTask IContactUnitOfWork.RollbackAsync(CancellationToken cancellationToken)
    {
        await GetTransaction()
            .RollbackAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    async ValueTask<int> IContactUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
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
