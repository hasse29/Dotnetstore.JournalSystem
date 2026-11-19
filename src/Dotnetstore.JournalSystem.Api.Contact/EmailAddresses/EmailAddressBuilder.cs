using Ardalis.GuardClauses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal sealed class EmailAddressBuilder :
    IEmailAddressWithCreateValues,
    IEmailAddressWithUpdateValues
{
    private Guid _id;
    private Guid _emailAddressTypeId;
    private Guid _objectId;
    private string _address = string.Empty;
    private DateTime _createdDate;
    private Guid? _createdBy;
    private DateTime? _lastUpdatedDate;
    private Guid? _lastUpdatedBy;
    private DateTime? _deletedDate;
    private Guid? _deletedBy;
    private bool _isDeleted;
    private bool _isSystem;
    private bool _isGdpr;

    private EmailAddressBuilder() { }

    internal static IEmailAddressWithCreateValues Create()
    {
        return new EmailAddressBuilder();
    }

    internal static IEmailAddressWithUpdateValues Update()
    {
        return new EmailAddressBuilder();
    }

    internal static EmailAddressBuilder Delete()
    {
        return new EmailAddressBuilder();
    }

    EmailAddressBuilder IEmailAddressWithCreateValues.WithCreateValues(EditEmailAddressRequest request, Guid? userId)
    {
        SetId(Guid.CreateVersion7());
        SetRequestValues(request);
        SetCreatedDate(DateTime.UtcNow);
        SetCreatedBy(userId);
        return this;
    }

    EmailAddressBuilder IEmailAddressWithUpdateValues.WithUpdateValues(EmailAddress existing, EditEmailAddressRequest request, Guid? userId)
    {
        SetEmailAddressValues(existing);
        SetRequestValues(request);
        SetLastUpdatedDate(DateTime.UtcNow);
        SetLastUpdatedBy(userId);
        return this;
    }

    private void SetRequestValues(EditEmailAddressRequest request)
    {
        SetEmailAddressTypeId(request.EmailAddressTypeId);
        SetObjectId(request.ObjectId);
        SetAddress(request.Address);
    }

    private void SetEmailAddressValues(EmailAddress emailAddress)
    {
        SetId(emailAddress.Id);
        SetEmailAddressTypeId(emailAddress.EmailAddressTypeId);
        SetObjectId(emailAddress.ObjectId);
        SetAddress(emailAddress.Address);
        SetCreatedDate(emailAddress.CreatedDate);
        SetCreatedBy(emailAddress.CreatedBy);
        SetLastUpdatedDate(emailAddress.LastUpdatedDate);
        SetLastUpdatedBy(emailAddress.LastUpdatedBy);
        SetDeletedDate(emailAddress.DeletedDate);
        SetDeletedBy(emailAddress.DeletedBy);
        SetIsDeleted(emailAddress.IsDeleted);
        SetIsSystem(emailAddress.IsSystem);
        SetIsGdpr(emailAddress.IsGdpr);
    }

    internal EmailAddressBuilder SetId(Guid id)
    {
        Guard.Against.Default(id);
        _id = id;
        return this;
    }

    internal EmailAddressBuilder SetEmailAddressTypeId(Guid emailAddressTypeId)
    {
        Guard.Against.Default(emailAddressTypeId);
        _emailAddressTypeId = emailAddressTypeId;
        return this;
    }

    internal EmailAddressBuilder SetObjectId(Guid objectId)
    {
        Guard.Against.Default(objectId);
        _objectId = objectId;
        return this;
    }

    internal EmailAddressBuilder SetAddress(string address)
    {
        Guard.Against.NullOrWhiteSpace(address);
        Guard.Against.StringTooLong(address, DataSchemeConstants.EmailAddressMaxLength);

        if (!EmailAddressFormat.IsValid(address))
        {
            throw new ArgumentException("The value must be a valid email address.", nameof(address));
        }

        _address = address;
        return this;
    }

    internal EmailAddressBuilder SetCreatedDate(DateTime createdDate)
    {
        Guard.Against.Default(createdDate);

        _createdDate = createdDate;
        return this;
    }

    internal EmailAddressBuilder SetCreatedBy(Guid? createdBy)
    {
        if (createdBy.HasValue)
        {
            Guard.Against.Default(createdBy.Value, nameof(createdBy));
        }

        _createdBy = createdBy;
        return this;
    }

    internal EmailAddressBuilder SetLastUpdatedDate(DateTime? lastUpdatedDate)
    {
        _lastUpdatedDate = lastUpdatedDate;
        return this;
    }

    internal EmailAddressBuilder SetLastUpdatedBy(Guid? lastUpdatedBy)
    {
        if (lastUpdatedBy.HasValue)
        {
            Guard.Against.Default(lastUpdatedBy.Value, nameof(lastUpdatedBy));
        }

        _lastUpdatedBy = lastUpdatedBy;
        return this;
    }

    internal EmailAddressBuilder SetDeletedDate(DateTime? deletedDate)
    {
        _deletedDate = deletedDate;
        return this;
    }

    internal EmailAddressBuilder SetDeletedBy(Guid? deletedBy)
    {
        if (deletedBy.HasValue)
        {
            Guard.Against.Default(deletedBy.Value, nameof(deletedBy));
        }

        _deletedBy = deletedBy;
        return this;
    }

    internal EmailAddressBuilder SetIsDeleted(bool isDeleted)
    {
        _isDeleted = isDeleted;
        return this;
    }

    internal EmailAddressBuilder SetIsSystem(bool isSystem)
    {
        _isSystem = isSystem;
        return this;
    }

    internal EmailAddressBuilder SetIsGdpr(bool isGdpr)
    {
        _isGdpr = isGdpr;
        return this;
    }

    internal EmailAddress Build()
    {
        return new EmailAddress(
            _id,
            _emailAddressTypeId,
            _objectId,
            _address,
            _createdDate,
            _createdBy,
            _lastUpdatedDate,
            _lastUpdatedBy,
            _deletedDate,
            _deletedBy,
            _isDeleted,
            _isSystem,
            _isGdpr);
    }
}

internal interface IEmailAddressWithCreateValues
{
    EmailAddressBuilder WithCreateValues(EditEmailAddressRequest request, Guid? userId);
}

internal interface IEmailAddressWithUpdateValues
{
    EmailAddressBuilder WithUpdateValues(EmailAddress existing, EditEmailAddressRequest request, Guid? userId);
}
