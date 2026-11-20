using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;

internal sealed class PhoneNumberTypeService(
    IContactUnitOfWork contactUnitOfWork) : IPhoneNumberTypeService
{
    async Task<IEnumerable<PhoneNumberTypeResponse>> IPhoneNumberTypeService.GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await contactUnitOfWork
            .PhoneNumberTypes
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.ToPhoneNumberTypeResponse());
    }

    async ValueTask<Result<PhoneNumberTypeResponse>> IPhoneNumberTypeService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var phoneNumberType = await contactUnitOfWork
            .PhoneNumberTypes
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return phoneNumberType is not null ?
            Result<PhoneNumberTypeResponse>.Success(phoneNumberType.ToPhoneNumberTypeResponse()) :
            Result<PhoneNumberTypeResponse>.NotFound();
    }

    async ValueTask<Result<PhoneNumberTypeResponse>> IPhoneNumberTypeService.CreateAsync(EditPhoneNumberTypeRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        var phoneNumberType = PhoneNumberTypeBuilder
            .Create()
            .WithCreateValues(request, userId)
            .Build();

        await contactUnitOfWork
            .PhoneNumberTypes
            .AddAsync(phoneNumberType, cancellationToken)
            .ConfigureAwait(false);

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<PhoneNumberTypeResponse>.Success(phoneNumberType.ToPhoneNumberTypeResponse()) :
            Result<PhoneNumberTypeResponse>.Error("Failed to create the phone number type.");
    }

    async ValueTask<Result<PhoneNumberTypeResponse>> IPhoneNumberTypeService.UpdateAsync(Guid id, EditPhoneNumberTypeRequest phoneNumberType, Guid? userId, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .PhoneNumberTypes
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result<PhoneNumberTypeResponse>.NotFound();
        }

        var phoneNumberTypeToUpdate = PhoneNumberTypeBuilder
            .Update()
            .WithUpdateValues(existing, phoneNumberType, userId)
            .Build();

        contactUnitOfWork
            .PhoneNumberTypes
            .Detach(existing);

        contactUnitOfWork
            .PhoneNumberTypes
            .Update(phoneNumberTypeToUpdate);

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<PhoneNumberTypeResponse>.Success(phoneNumberTypeToUpdate.ToPhoneNumberTypeResponse()) :
            Result<PhoneNumberTypeResponse>.Error("Failed to update the phone number type.");
    }

    async ValueTask<Result> IPhoneNumberTypeService.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .PhoneNumberTypes
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.NotFound();
        }

        var inUse = await contactUnitOfWork
            .PhoneNumbers
            .AnyWithPhoneNumberTypeAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (inUse)
        {
            return Result.Conflict("The phone number type is in use by one or more phone numbers and cannot be deleted.");
        }

        await contactUnitOfWork
            .PhoneNumberTypes
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result.Success() :
            Result.Error("Failed to delete the phone number type.");
    }
}