using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;

internal sealed class EmailAddressTypeService(
    IContactUnitOfWork contactUnitOfWork) : IEmailAddressTypeService
{
    async Task<IEnumerable<EmailAddressTypeResponse>> IEmailAddressTypeService.GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await contactUnitOfWork
            .EmailAddressTypes
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.ToEmailAddressTypeResponse());
    }

    async ValueTask<Result<EmailAddressTypeResponse>> IEmailAddressTypeService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var emailAddressType = await contactUnitOfWork
            .EmailAddressTypes
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return emailAddressType is not null ?
            Result<EmailAddressTypeResponse>.Success(emailAddressType.ToEmailAddressTypeResponse()) :
            Result<EmailAddressTypeResponse>.NotFound();
    }

    async ValueTask<Result<EmailAddressTypeResponse>> IEmailAddressTypeService.CreateAsync(EditEmailAddressTypeRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        var emailAddressType = EmailAddressTypeBuilder
            .Create()
            .WithCreateValues(request, userId)
            .Build();

        await contactUnitOfWork
            .EmailAddressTypes
            .AddAsync(emailAddressType, cancellationToken)
            .ConfigureAwait(false);

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<EmailAddressTypeResponse>.Success(emailAddressType.ToEmailAddressTypeResponse()) :
            Result<EmailAddressTypeResponse>.Error("Failed to create the email address type.");
    }

    async ValueTask<Result<EmailAddressTypeResponse>> IEmailAddressTypeService.UpdateAsync(Guid id, EditEmailAddressTypeRequest emailAddressType, Guid? userId, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .EmailAddressTypes
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result<EmailAddressTypeResponse>.NotFound();
        }

        var emailAddressTypeToUpdate = EmailAddressTypeBuilder
            .Update()
            .WithUpdateValues(existing, emailAddressType, userId)
            .Build();

        contactUnitOfWork
            .EmailAddressTypes
            .Detach(existing);

        contactUnitOfWork
            .EmailAddressTypes
            .Update(emailAddressTypeToUpdate);

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<EmailAddressTypeResponse>.Success(emailAddressTypeToUpdate.ToEmailAddressTypeResponse()) :
            Result<EmailAddressTypeResponse>.Error("Failed to update the email address type.");
    }

    async ValueTask<Result> IEmailAddressTypeService.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await contactUnitOfWork
            .EmailAddressTypes
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.NotFound();
        }

        var inUse = await contactUnitOfWork
            .EmailAddresses
            .AnyWithEmailAddressTypeAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (inUse)
        {
            return Result.Conflict("The email address type is in use by one or more email addresses and cannot be deleted.");
        }

        await contactUnitOfWork
            .EmailAddressTypes
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        var result = await contactUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result.Success() :
            Result.Error("Failed to delete the email address type.");
    }
}