using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Dotnetstore.Security.Handlers;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal sealed class EmailAddressService(
    IContactUnitOfWork contactUnitOfWork,
    IEncryptionHandler encryption) : IEmailAddressService
{
    async Task<IEnumerable<EmailAddressResponse>> IEmailAddressService.GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await contactUnitOfWork
            .EmailAddresses
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.Decrypt(encryption).ToEmailAddressResponse());
    }

    async ValueTask<Result<EmailAddressResponse>> IEmailAddressService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var emailAddress = await contactUnitOfWork
            .EmailAddresses
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return emailAddress is not null ?
            Result<EmailAddressResponse>.Success(emailAddress.Decrypt(encryption).ToEmailAddressResponse()) :
            Result<EmailAddressResponse>.NotFound();
    }

    async ValueTask<Result<EmailAddressResponse>> IEmailAddressService.CreateAsync(EditEmailAddressRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        if (!await EmailAddressTypeExistsAsync(request.EmailAddressTypeId, cancellationToken).ConfigureAwait(false))
        {
            return Result<EmailAddressResponse>.Invalid(UnknownEmailAddressType());
        }

        var emailAddress = EmailAddressBuilder
            .Create()
            .WithCreateValues(request, userId)
            .Build();

        await contactUnitOfWork
            .EmailAddresses
            .AddAsync(emailAddress.Encrypt(encryption), cancellationToken)
            .ConfigureAwait(false);

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<EmailAddressResponse>.Success(emailAddress.ToEmailAddressResponse()) :
            Result<EmailAddressResponse>.Error("Failed to create the email address.");
    }

    async ValueTask<Result<EmailAddressResponse>> IEmailAddressService.UpdateAsync(Guid id, EditEmailAddressRequest emailAddress, Guid? userId, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .EmailAddresses
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result<EmailAddressResponse>.NotFound();
        }

        if (!await EmailAddressTypeExistsAsync(emailAddress.EmailAddressTypeId, cancellationToken).ConfigureAwait(false))
        {
            return Result<EmailAddressResponse>.Invalid(UnknownEmailAddressType());
        }

        var emailAddressToUpdate = EmailAddressBuilder
            .Update()
            .WithUpdateValues(existing, emailAddress, userId)
            .Build();

        contactUnitOfWork
            .EmailAddresses
            .Detach(existing);

        contactUnitOfWork
            .EmailAddresses
            .Update(emailAddressToUpdate.Encrypt(encryption));

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<EmailAddressResponse>.Success(emailAddressToUpdate.ToEmailAddressResponse()) :
            Result<EmailAddressResponse>.Error("Failed to update the email address.");
    }

    async ValueTask<Result> IEmailAddressService.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .EmailAddresses
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.NotFound();
        }

        await contactUnitOfWork
            .EmailAddresses
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result.Success() :
            Result.Error("Failed to delete the email address.");
    }

    private async Task<bool> EmailAddressTypeExistsAsync(Guid emailAddressTypeId, CancellationToken cancellationToken)
    {
        var emailAddressType = await contactUnitOfWork
            .EmailAddressTypes
            .GetByIdAsync(emailAddressTypeId, cancellationToken)
            .ConfigureAwait(false);

        return emailAddressType is not null;
    }

    private static ValidationError UnknownEmailAddressType()
    {
        return new ValidationError(nameof(EditEmailAddressRequest.EmailAddressTypeId), "The email address type does not exist.");
    }
}
