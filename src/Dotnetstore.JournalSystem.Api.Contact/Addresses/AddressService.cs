using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal sealed class AddressService(
    IContactUnitOfWork contactUnitOfWork) : IAddressService
{
    async Task<IEnumerable<AddressResponse>> IAddressService.GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await contactUnitOfWork
            .Addresses
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.ToAddressResponse());
    }

    async ValueTask<Result<AddressResponse>> IAddressService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var address = await contactUnitOfWork
            .Addresses
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return address is not null ?
            Result<AddressResponse>.Success(address.ToAddressResponse()) :
            Result<AddressResponse>.NotFound();
    }

    async ValueTask<Result<AddressResponse>> IAddressService.CreateAsync(EditAddressRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        if (!await AddressTypeExistsAsync(request.AddressTypeId, cancellationToken).ConfigureAwait(false))
        {
            return Result<AddressResponse>.Invalid(UnknownAddressType());
        }

        var address = AddressBuilder
            .Create()
            .WithCreateValues(request, userId)
            .Build();
        
        await contactUnitOfWork
            .Addresses
            .AddAsync(address, cancellationToken)
            .ConfigureAwait(false);
        
        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
        
        return result > 0 ?
            Result<AddressResponse>.Success(address.ToAddressResponse()) :
            Result<AddressResponse>.Error("Failed to create the address.");
    }

    async ValueTask<Result<AddressResponse>> IAddressService.UpdateAsync(Guid id, EditAddressRequest address, Guid? userId, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .Addresses
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);
        
        if (existing is null)
        {
            return Result<AddressResponse>.NotFound();
        }

        if (!await AddressTypeExistsAsync(address.AddressTypeId, cancellationToken).ConfigureAwait(false))
        {
            return Result<AddressResponse>.Invalid(UnknownAddressType());
        }
        
        var addressToUpdate = AddressBuilder
            .Update()
            .WithUpdateValues(existing, address, userId)
            .Build();
        
        contactUnitOfWork
            .Addresses
            .Detach(existing);

        contactUnitOfWork
            .Addresses
            .Update(addressToUpdate);
        
        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
        
        return result > 0 ?
            Result<AddressResponse>.Success(addressToUpdate.ToAddressResponse()) :
            Result<AddressResponse>.Error("Failed to update the address.");
    }

    async ValueTask<Result> IAddressService.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .Addresses
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.NotFound();
        }

        await contactUnitOfWork
            .Addresses
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);
        
        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
        
        return result > 0 ? 
            Result.Success() : 
            Result.Error("Failed to delete the address.");
    }

    private async Task<bool> AddressTypeExistsAsync(Guid addressTypeId, CancellationToken cancellationToken)
    {
        var addressType = await contactUnitOfWork
            .AddressTypes
            .GetByIdAsync(addressTypeId, cancellationToken)
            .ConfigureAwait(false);

        return addressType is not null;
    }

    private static ValidationError UnknownAddressType()
    {
        return new ValidationError(nameof(EditAddressRequest.AddressTypeId), "The address type does not exist.");
    }
}
