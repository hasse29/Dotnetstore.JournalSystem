using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.AddressTypes;

internal sealed class AddressTypeService(
    IContactUnitOfWork contactUnitOfWork) : IAddressTypeService
{
    async Task<IEnumerable<AddressTypeResponse>> IAddressTypeService.GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await contactUnitOfWork
            .AddressTypes
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.ToAddressTypeResponse());
    }

    async ValueTask<Result<AddressTypeResponse>> IAddressTypeService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var addressType = await contactUnitOfWork
            .AddressTypes
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return addressType is not null ?
            Result<AddressTypeResponse>.Success(addressType.ToAddressTypeResponse()) :
            Result<AddressTypeResponse>.NotFound();
    }

    async ValueTask<Result<AddressTypeResponse>> IAddressTypeService.CreateAsync(EditAddressTypeRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        var addressType = AddressTypeBuilder
            .Create()
            .WithCreateValues(request, userId)
            .Build();
        
        await contactUnitOfWork
            .AddressTypes
            .AddAsync(addressType, cancellationToken)
            .ConfigureAwait(false);
        
        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
        
        return result > 0 ?
            Result<AddressTypeResponse>.Success(addressType.ToAddressTypeResponse()) :
            Result<AddressTypeResponse>.Error("Failed to create the address type.");
    }

    async ValueTask<Result<AddressTypeResponse>> IAddressTypeService.UpdateAsync(Guid id, EditAddressTypeRequest addressType, Guid? userId, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .AddressTypes
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);
        
        if (existing is null)
        {
            return Result<AddressTypeResponse>.NotFound();
        }
        
        var addressTypeToUpdate = AddressTypeBuilder
            .Update()
            .WithUpdateValues(existing, addressType, userId)
            .Build();
        
        contactUnitOfWork
            .AddressTypes
            .Detach(existing);

        contactUnitOfWork
            .AddressTypes
            .Update(addressTypeToUpdate);
        
        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
        
        return result > 0 ?
            Result<AddressTypeResponse>.Success(addressTypeToUpdate.ToAddressTypeResponse()) :
            Result<AddressTypeResponse>.Error("Failed to update the address type.");
    }

    async ValueTask<Result> IAddressTypeService.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .AddressTypes
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.NotFound();
        }

        await contactUnitOfWork
            .AddressTypes
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);
        
        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
        
        return result > 0 ? 
            Result.Success() : 
            Result.Error("Failed to delete the address type.");
    }
}