using Ardalis.GuardClauses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;

internal sealed class PhoneNumberTypeBuilder :
    IPhoneNumberTypeWithCreateValues,
    IPhoneNumberTypeWithUpdateValues
{
    private Guid _id;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private DateTime _createdDate;
    private Guid? _createdBy;
    private DateTime? _lastUpdatedDate;
    private Guid? _lastUpdatedBy;
    private DateTime? _deletedDate;
    private Guid? _deletedBy;
    private bool _isDeleted;
    private bool _isSystem;
    private bool _isGdpr;

    private PhoneNumberTypeBuilder() { }

    internal static IPhoneNumberTypeWithCreateValues Create()
    {
        return new PhoneNumberTypeBuilder();
    }

    internal static IPhoneNumberTypeWithUpdateValues Update()
    {
        return new PhoneNumberTypeBuilder();
    }

    internal static PhoneNumberTypeBuilder Delete()
    {
        return new PhoneNumberTypeBuilder();
    }

    PhoneNumberTypeBuilder IPhoneNumberTypeWithCreateValues.WithCreateValues(EditPhoneNumberTypeRequest request, Guid? userId)
    {
        SetId(Guid.CreateVersion7());
        SetName(request.Name);
        SetDescription(request.Description);
        SetCreatedDate(DateTime.UtcNow);
        SetCreatedBy(userId);
        return this;
    }

    PhoneNumberTypeBuilder IPhoneNumberTypeWithUpdateValues.WithUpdateValues(PhoneNumberType existing, EditPhoneNumberTypeRequest request, Guid? userId)
    {
        SetPhoneNumberTypeValues(existing);
        SetName(request.Name);
        SetDescription(request.Description);
        SetLastUpdatedDate(DateTime.UtcNow);
        SetLastUpdatedBy(userId);
        return this;
    }

    private void SetPhoneNumberTypeValues(PhoneNumberType phoneNumberType)
    {
        SetId(phoneNumberType.Id);
        SetName(phoneNumberType.Name);
        SetDescription(phoneNumberType.Description);
        SetCreatedDate(phoneNumberType.CreatedDate);
        SetCreatedBy(phoneNumberType.CreatedBy);
        SetLastUpdatedDate(phoneNumberType.LastUpdatedDate);
        SetLastUpdatedBy(phoneNumberType.LastUpdatedBy);
        SetDeletedDate(phoneNumberType.DeletedDate);
        SetDeletedBy(phoneNumberType.DeletedBy);
        SetIsDeleted(phoneNumberType.IsDeleted);
        SetIsSystem(phoneNumberType.IsSystem);
        SetIsGdpr(phoneNumberType.IsGdpr);
    }

    internal PhoneNumberTypeBuilder SetId(Guid id)
    {
        Guard.Against.Default(id);
        _id = id;
        return this;
    }

    internal PhoneNumberTypeBuilder SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.StringTooLong(name, DataSchemeConstants.PhoneNumberTypeNameMaxLength);

        _name = name;
        return this;
    }

    internal PhoneNumberTypeBuilder SetDescription(string description)
    {
        Guard.Against.Null(description);
        Guard.Against.StringTooLong(description, DataSchemeConstants.PhoneNumberTypeDescriptionMaxLength);

        _description = description;
        return this;
    }

    internal PhoneNumberTypeBuilder SetCreatedDate(DateTime createdDate)
    {
        Guard.Against.Default(createdDate);

        _createdDate = createdDate;
        return this;
    }

    internal PhoneNumberTypeBuilder SetCreatedBy(Guid? createdBy)
    {
        if (createdBy.HasValue)
        {
            Guard.Against.Default(createdBy.Value, nameof(createdBy));
        }

        _createdBy = createdBy;
        return this;
    }

    internal PhoneNumberTypeBuilder SetLastUpdatedDate(DateTime? lastUpdatedDate)
    {
        _lastUpdatedDate = lastUpdatedDate;
        return this;
    }

    internal PhoneNumberTypeBuilder SetLastUpdatedBy(Guid? lastUpdatedBy)
    {
        if (lastUpdatedBy.HasValue)
        {
            Guard.Against.Default(lastUpdatedBy.Value, nameof(lastUpdatedBy));
        }

        _lastUpdatedBy = lastUpdatedBy;
        return this;
    }

    internal PhoneNumberTypeBuilder SetDeletedDate(DateTime? deletedDate)
    {
        _deletedDate = deletedDate;
        return this;
    }

    internal PhoneNumberTypeBuilder SetDeletedBy(Guid? deletedBy)
    {
        if (deletedBy.HasValue)
        {
            Guard.Against.Default(deletedBy.Value, nameof(deletedBy));
        }

        _deletedBy = deletedBy;
        return this;
    }

    internal PhoneNumberTypeBuilder SetIsDeleted(bool isDeleted)
    {
        _isDeleted = isDeleted;
        return this;
    }

    internal PhoneNumberTypeBuilder SetIsSystem(bool isSystem)
    {
        _isSystem = isSystem;
        return this;
    }

    internal PhoneNumberTypeBuilder SetIsGdpr(bool isGdpr)
    {
        _isGdpr = isGdpr;
        return this;
    }

    internal PhoneNumberType Build()
    {
        return new PhoneNumberType(
            _id,
            _name,
            _description,
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

internal interface IPhoneNumberTypeWithCreateValues
{
    PhoneNumberTypeBuilder WithCreateValues(EditPhoneNumberTypeRequest request, Guid? userId);
}

internal interface IPhoneNumberTypeWithUpdateValues
{
    PhoneNumberTypeBuilder WithUpdateValues(PhoneNumberType existing, EditPhoneNumberTypeRequest request, Guid? userId);
}
