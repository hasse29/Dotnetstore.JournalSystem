using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Dotnetstore.Security.Handlers;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal sealed class PhoneNumberService(
    IContactUnitOfWork contactUnitOfWork,
    IEncryptionHandler encryption) : IPhoneNumberService
{
    async Task<IEnumerable<PhoneNumberResponse>> IPhoneNumberService.GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await contactUnitOfWork
            .PhoneNumbers
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.Decrypt(encryption).ToPhoneNumberResponse());
    }

    async ValueTask<Result<PhoneNumberResponse>> IPhoneNumberService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var phoneNumber = await contactUnitOfWork
            .PhoneNumbers
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return phoneNumber is not null ?
            Result<PhoneNumberResponse>.Success(phoneNumber.Decrypt(encryption).ToPhoneNumberResponse()) :
            Result<PhoneNumberResponse>.NotFound();
    }

    async ValueTask<Result<PhoneNumberResponse>> IPhoneNumberService.CreateAsync(EditPhoneNumberRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        if (!await PhoneNumberTypeExistsAsync(request.PhoneNumberTypeId, cancellationToken).ConfigureAwait(false))
        {
            return Result<PhoneNumberResponse>.Invalid(UnknownPhoneNumberType());
        }

        var phoneNumber = PhoneNumberBuilder
            .Create()
            .WithCreateValues(request, userId)
            .Build();

        await contactUnitOfWork
            .PhoneNumbers
            .AddAsync(phoneNumber.Encrypt(encryption), cancellationToken)
            .ConfigureAwait(false);

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<PhoneNumberResponse>.Success(phoneNumber.ToPhoneNumberResponse()) :
            Result<PhoneNumberResponse>.Error("Failed to create the phone number.");
    }

    async ValueTask<Result<PhoneNumberResponse>> IPhoneNumberService.UpdateAsync(Guid id, EditPhoneNumberRequest phoneNumber, Guid? userId, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .PhoneNumbers
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result<PhoneNumberResponse>.NotFound();
        }

        if (!await PhoneNumberTypeExistsAsync(phoneNumber.PhoneNumberTypeId, cancellationToken).ConfigureAwait(false))
        {
            return Result<PhoneNumberResponse>.Invalid(UnknownPhoneNumberType());
        }

        var phoneNumberToUpdate = PhoneNumberBuilder
            .Update()
            .WithUpdateValues(existing, phoneNumber, userId)
            .Build();

        contactUnitOfWork
            .PhoneNumbers
            .Detach(existing);

        contactUnitOfWork
            .PhoneNumbers
            .Update(phoneNumberToUpdate.Encrypt(encryption));

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<PhoneNumberResponse>.Success(phoneNumberToUpdate.ToPhoneNumberResponse()) :
            Result<PhoneNumberResponse>.Error("Failed to update the phone number.");
    }

    async ValueTask<Result> IPhoneNumberService.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .PhoneNumbers
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.NotFound();
        }

        await contactUnitOfWork
            .PhoneNumbers
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result.Success() :
            Result.Error("Failed to delete the phone number.");
    }

    private async Task<bool> PhoneNumberTypeExistsAsync(Guid phoneNumberTypeId, CancellationToken cancellationToken)
    {
        var phoneNumberType = await contactUnitOfWork
            .PhoneNumberTypes
            .GetByIdAsync(phoneNumberTypeId, cancellationToken)
            .ConfigureAwait(false);

        return phoneNumberType is not null;
    }

    private static ValidationError UnknownPhoneNumberType()
    {
        return new ValidationError(nameof(EditPhoneNumberRequest.PhoneNumberTypeId), "The phone number type does not exist.");
    }
}
