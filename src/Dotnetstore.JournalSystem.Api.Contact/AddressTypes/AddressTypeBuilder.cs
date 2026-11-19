using Ardalis.GuardClauses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.AddressTypes;

internal sealed class AddressTypeBuilder :
    IAddressTypeWithCreateValues,
    IAddressTypeWithUpdateValues
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

    private AddressTypeBuilder() { }

    internal static IAddressTypeWithCreateValues Create()
    {
        return new AddressTypeBuilder();
    }

    internal static IAddressTypeWithUpdateValues Update()
    {
        return new AddressTypeBuilder();
    }

    internal static AddressTypeBuilder Delete()
    {
        return new AddressTypeBuilder();
    }

    AddressTypeBuilder IAddressTypeWithCreateValues.WithCreateValues(EditAddressTypeRequest request, Guid? userId)
    {
        SetId(Guid.CreateVersion7());
        SetName(request.Name);
        SetDescription(request.Description);
        SetCreatedDate(DateTime.UtcNow);
        SetCreatedBy(userId);
        return this;
    }

    AddressTypeBuilder IAddressTypeWithUpdateValues.WithUpdateValues(AddressType existing, EditAddressTypeRequest request, Guid? userId)
    {
        SetAddressTypeValues(existing);
        SetName(request.Name);
        SetDescription(request.Description);
        SetLastUpdatedDate(DateTime.UtcNow);
        SetLastUpdatedBy(userId);
        return this;
    }

    private void SetAddressTypeValues(AddressType addressType)
    {
        SetId(addressType.Id);
        SetName(addressType.Name);
        SetDescription(addressType.Description);
        SetCreatedDate(addressType.CreatedDate);
        SetCreatedBy(addressType.CreatedBy);
        SetLastUpdatedDate(addressType.LastUpdatedDate);
        SetLastUpdatedBy(addressType.LastUpdatedBy);
        SetDeletedDate(addressType.DeletedDate);
        SetDeletedBy(addressType.DeletedBy);
        SetIsDeleted(addressType.IsDeleted);
        SetIsSystem(addressType.IsSystem);
        SetIsGdpr(addressType.IsGdpr);
    }

    internal AddressTypeBuilder SetId(Guid id)
    {
        Guard.Against.Default(id);
        _id = id;
        return this;
    }

    internal AddressTypeBuilder SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.StringTooLong(name, DataSchemeConstants.AddressTypeNameMaxLength);

        _name = name;
        return this;
    }

    internal AddressTypeBuilder SetDescription(string description)
    {
        Guard.Against.Null(description);
        Guard.Against.StringTooLong(description, DataSchemeConstants.AddressTypeDescriptionMaxLength);

        _description = description;
        return this;
    }

    internal AddressTypeBuilder SetCreatedDate(DateTime createdDate)
    {
        Guard.Against.Default(createdDate);

        _createdDate = createdDate;
        return this;
    }

    internal AddressTypeBuilder SetCreatedBy(Guid? createdBy)
    {
        if (createdBy.HasValue)
        {
            Guard.Against.Default(createdBy.Value, nameof(createdBy));
        }

        _createdBy = createdBy;
        return this;
    }

    internal AddressTypeBuilder SetLastUpdatedDate(DateTime? lastUpdatedDate)
    {
        _lastUpdatedDate = lastUpdatedDate;
        return this;
    }

    internal AddressTypeBuilder SetLastUpdatedBy(Guid? lastUpdatedBy)
    {
        if (lastUpdatedBy.HasValue)
        {
            Guard.Against.Default(lastUpdatedBy.Value, nameof(lastUpdatedBy));
        }

        _lastUpdatedBy = lastUpdatedBy;
        return this;
    }

    internal AddressTypeBuilder SetDeletedDate(DateTime? deletedDate)
    {
        _deletedDate = deletedDate;
        return this;
    }

    internal AddressTypeBuilder SetDeletedBy(Guid? deletedBy)
    {
        if (deletedBy.HasValue)
        {
            Guard.Against.Default(deletedBy.Value, nameof(deletedBy));
        }

        _deletedBy = deletedBy;
        return this;
    }

    internal AddressTypeBuilder SetIsDeleted(bool isDeleted)
    {
        _isDeleted = isDeleted;
        return this;
    }

    internal AddressTypeBuilder SetIsSystem(bool isSystem)
    {
        _isSystem = isSystem;
        return this;
    }

    internal AddressTypeBuilder SetIsGdpr(bool isGdpr)
    {
        _isGdpr = isGdpr;
        return this;
    }

    internal AddressType Build()
    {
        return new AddressType(
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

internal interface IAddressTypeWithCreateValues
{
    AddressTypeBuilder WithCreateValues(EditAddressTypeRequest request, Guid? userId);
}

internal interface IAddressTypeWithUpdateValues
{
    AddressTypeBuilder WithUpdateValues(AddressType existing, EditAddressTypeRequest request, Guid? userId);
}
