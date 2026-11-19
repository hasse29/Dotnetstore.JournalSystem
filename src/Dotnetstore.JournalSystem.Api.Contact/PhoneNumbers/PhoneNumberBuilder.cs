using Ardalis.GuardClauses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal sealed class PhoneNumberBuilder :
    IPhoneNumberWithCreateValues,
    IPhoneNumberWithUpdateValues
{
    private Guid _id;
    private Guid _phoneNumberTypeId;
    private Guid _objectId;
    private string _number = string.Empty;
    private DateTime _createdDate;
    private Guid? _createdBy;
    private DateTime? _lastUpdatedDate;
    private Guid? _lastUpdatedBy;
    private DateTime? _deletedDate;
    private Guid? _deletedBy;
    private bool _isDeleted;
    private bool _isSystem;
    private bool _isGdpr;

    private PhoneNumberBuilder() { }

    internal static IPhoneNumberWithCreateValues Create()
    {
        return new PhoneNumberBuilder();
    }

    internal static IPhoneNumberWithUpdateValues Update()
    {
        return new PhoneNumberBuilder();
    }

    internal static PhoneNumberBuilder Delete()
    {
        return new PhoneNumberBuilder();
    }

    PhoneNumberBuilder IPhoneNumberWithCreateValues.WithCreateValues(EditPhoneNumberRequest request, Guid? userId)
    {
        SetId(Guid.CreateVersion7());
        SetRequestValues(request);
        SetCreatedDate(DateTime.UtcNow);
        SetCreatedBy(userId);
        return this;
    }

    PhoneNumberBuilder IPhoneNumberWithUpdateValues.WithUpdateValues(PhoneNumber existing, EditPhoneNumberRequest request, Guid? userId)
    {
        SetPhoneNumberValues(existing);
        SetRequestValues(request);
        SetLastUpdatedDate(DateTime.UtcNow);
        SetLastUpdatedBy(userId);
        return this;
    }

    private void SetRequestValues(EditPhoneNumberRequest request)
    {
        SetPhoneNumberTypeId(request.PhoneNumberTypeId);
        SetObjectId(request.ObjectId);
        SetNumber(request.Number);
    }

    private void SetPhoneNumberValues(PhoneNumber phoneNumber)
    {
        SetId(phoneNumber.Id);
        SetPhoneNumberTypeId(phoneNumber.PhoneNumberTypeId);
        SetObjectId(phoneNumber.ObjectId);
        SetNumber(phoneNumber.Number);
        SetCreatedDate(phoneNumber.CreatedDate);
        SetCreatedBy(phoneNumber.CreatedBy);
        SetLastUpdatedDate(phoneNumber.LastUpdatedDate);
        SetLastUpdatedBy(phoneNumber.LastUpdatedBy);
        SetDeletedDate(phoneNumber.DeletedDate);
        SetDeletedBy(phoneNumber.DeletedBy);
        SetIsDeleted(phoneNumber.IsDeleted);
        SetIsSystem(phoneNumber.IsSystem);
        SetIsGdpr(phoneNumber.IsGdpr);
    }

    internal PhoneNumberBuilder SetId(Guid id)
    {
        Guard.Against.Default(id);
        _id = id;
        return this;
    }

    internal PhoneNumberBuilder SetPhoneNumberTypeId(Guid phoneNumberTypeId)
    {
        Guard.Against.Default(phoneNumberTypeId);
        _phoneNumberTypeId = phoneNumberTypeId;
        return this;
    }

    internal PhoneNumberBuilder SetObjectId(Guid objectId)
    {
        Guard.Against.Default(objectId);
        _objectId = objectId;
        return this;
    }

    internal PhoneNumberBuilder SetNumber(string number)
    {
        Guard.Against.NullOrWhiteSpace(number);
        Guard.Against.StringTooLong(number, DataSchemeConstants.PhoneNumberMaxLength);

        if (!PhoneNumberFormat.IsValid(number))
        {
            throw new ArgumentException("The value must be a valid phone number.", nameof(number));
        }

        _number = number;
        return this;
    }

    internal PhoneNumberBuilder SetCreatedDate(DateTime createdDate)
    {
        Guard.Against.Default(createdDate);

        _createdDate = createdDate;
        return this;
    }

    internal PhoneNumberBuilder SetCreatedBy(Guid? createdBy)
    {
        if (createdBy.HasValue)
        {
            Guard.Against.Default(createdBy.Value, nameof(createdBy));
        }

        _createdBy = createdBy;
        return this;
    }

    internal PhoneNumberBuilder SetLastUpdatedDate(DateTime? lastUpdatedDate)
    {
        _lastUpdatedDate = lastUpdatedDate;
        return this;
    }

    internal PhoneNumberBuilder SetLastUpdatedBy(Guid? lastUpdatedBy)
    {
        if (lastUpdatedBy.HasValue)
        {
            Guard.Against.Default(lastUpdatedBy.Value, nameof(lastUpdatedBy));
        }

        _lastUpdatedBy = lastUpdatedBy;
        return this;
    }

    internal PhoneNumberBuilder SetDeletedDate(DateTime? deletedDate)
    {
        _deletedDate = deletedDate;
        return this;
    }

    internal PhoneNumberBuilder SetDeletedBy(Guid? deletedBy)
    {
        if (deletedBy.HasValue)
        {
            Guard.Against.Default(deletedBy.Value, nameof(deletedBy));
        }

        _deletedBy = deletedBy;
        return this;
    }

    internal PhoneNumberBuilder SetIsDeleted(bool isDeleted)
    {
        _isDeleted = isDeleted;
        return this;
    }

    internal PhoneNumberBuilder SetIsSystem(bool isSystem)
    {
        _isSystem = isSystem;
        return this;
    }

    internal PhoneNumberBuilder SetIsGdpr(bool isGdpr)
    {
        _isGdpr = isGdpr;
        return this;
    }

    internal PhoneNumber Build()
    {
        return new PhoneNumber(
            _id,
            _phoneNumberTypeId,
            _objectId,
            _number,
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

internal interface IPhoneNumberWithCreateValues
{
    PhoneNumberBuilder WithCreateValues(EditPhoneNumberRequest request, Guid? userId);
}

internal interface IPhoneNumberWithUpdateValues
{
    PhoneNumberBuilder WithUpdateValues(PhoneNumber existing, EditPhoneNumberRequest request, Guid? userId);
}
