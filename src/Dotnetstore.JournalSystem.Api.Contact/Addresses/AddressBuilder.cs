using Ardalis.GuardClauses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal sealed class AddressBuilder :
    IAddressWithCreateValues,
    IAddressWithUpdateValues
{
    private Guid _id;
    private Guid _addressTypeId;
    private Guid _objectId;
    private string? _co;
    private string _street1 = string.Empty;
    private string? _street2;
    private string _postalCode = string.Empty;
    private string _city = string.Empty;
    private string _state = string.Empty;
    private string _country = string.Empty;
    private DateTime _createdDate;
    private Guid? _createdBy;
    private DateTime? _lastUpdatedDate;
    private Guid? _lastUpdatedBy;
    private DateTime? _deletedDate;
    private Guid? _deletedBy;
    private bool _isDeleted;
    private bool _isSystem;
    private bool _isGdpr;

    private AddressBuilder() { }

    internal static IAddressWithCreateValues Create()
    {
        return new AddressBuilder();
    }

    internal static IAddressWithUpdateValues Update()
    {
        return new AddressBuilder();
    }

    internal static AddressBuilder Delete()
    {
        return new AddressBuilder();
    }

    AddressBuilder IAddressWithCreateValues.WithCreateValues(EditAddressRequest request, Guid? userId)
    {
        SetId(Guid.CreateVersion7());
        SetRequestValues(request);
        SetCreatedDate(DateTime.UtcNow);
        SetCreatedBy(userId);
        return this;
    }

    AddressBuilder IAddressWithUpdateValues.WithUpdateValues(Address existing, EditAddressRequest request, Guid? userId)
    {
        SetAddressValues(existing);
        SetRequestValues(request);
        SetLastUpdatedDate(DateTime.UtcNow);
        SetLastUpdatedBy(userId);
        return this;
    }

    private void SetRequestValues(EditAddressRequest request)
    {
        SetAddressTypeId(request.AddressTypeId);
        SetObjectId(request.ObjectId);
        SetCO(request.CO);
        SetStreet1(request.Street1);
        SetStreet2(request.Street2);
        SetPostalCode(request.PostalCode);
        SetCity(request.City);
        SetState(request.State);
        SetCountry(request.Country);
    }

    // Sensitive values on an existing row are ciphertext, so they are never copied; the request supplies the new plaintext.
    private void SetAddressValues(Address address)
    {
        SetId(address.Id);
        SetAddressTypeId(address.AddressTypeId);
        SetObjectId(address.ObjectId);
        SetCreatedDate(address.CreatedDate);
        SetCreatedBy(address.CreatedBy);
        SetLastUpdatedDate(address.LastUpdatedDate);
        SetLastUpdatedBy(address.LastUpdatedBy);
        SetDeletedDate(address.DeletedDate);
        SetDeletedBy(address.DeletedBy);
        SetIsDeleted(address.IsDeleted);
        SetIsSystem(address.IsSystem);
        SetIsGdpr(address.IsGdpr);
    }

    internal AddressBuilder SetId(Guid id)
    {
        Guard.Against.Default(id);
        _id = id;
        return this;
    }

    internal AddressBuilder SetAddressTypeId(Guid addressTypeId)
    {
        Guard.Against.Default(addressTypeId);
        _addressTypeId = addressTypeId;
        return this;
    }

    internal AddressBuilder SetObjectId(Guid objectId)
    {
        Guard.Against.Default(objectId);
        _objectId = objectId;
        return this;
    }

    internal AddressBuilder SetCO(string? co)
    {
        if (!string.IsNullOrWhiteSpace(co))
        {
            Guard.Against.StringTooLong(co, DataSchemeConstants.AddressCOMaxLength);
        }

        _co = co;
        return this;
    }

    internal AddressBuilder SetStreet1(string street1)
    {
        Guard.Against.NullOrWhiteSpace(street1);
        Guard.Against.StringTooLong(street1, DataSchemeConstants.AddressStreetMaxLength);

        _street1 = street1;
        return this;
    }

    internal AddressBuilder SetStreet2(string? street2)
    {
        if (!string.IsNullOrWhiteSpace(street2))
        {
            Guard.Against.StringTooLong(street2, DataSchemeConstants.AddressStreetMaxLength);
        }

        _street2 = street2;
        return this;
    }

    internal AddressBuilder SetPostalCode(string postalCode)
    {
        Guard.Against.NullOrWhiteSpace(postalCode);
        Guard.Against.StringTooLong(postalCode, DataSchemeConstants.AddressPostalCodeMaxLength);

        _postalCode = postalCode;
        return this;
    }

    internal AddressBuilder SetCity(string city)
    {
        Guard.Against.NullOrWhiteSpace(city);
        Guard.Against.StringTooLong(city, DataSchemeConstants.AddressCityMaxLength);

        _city = city;
        return this;
    }

    internal AddressBuilder SetState(string state)
    {
        Guard.Against.Null(state);
        Guard.Against.StringTooLong(state, DataSchemeConstants.AddressStateMaxLength);

        _state = state;
        return this;
    }

    internal AddressBuilder SetCountry(string country)
    {
        Guard.Against.NullOrWhiteSpace(country);
        Guard.Against.StringTooLong(country, DataSchemeConstants.AddressCountryMaxLength);

        _country = country;
        return this;
    }

    internal AddressBuilder SetCreatedDate(DateTime createdDate)
    {
        Guard.Against.Default(createdDate);

        _createdDate = createdDate;
        return this;
    }

    internal AddressBuilder SetCreatedBy(Guid? createdBy)
    {
        if (createdBy.HasValue)
        {
            Guard.Against.Default(createdBy.Value, nameof(createdBy));
        }

        _createdBy = createdBy;
        return this;
    }

    internal AddressBuilder SetLastUpdatedDate(DateTime? lastUpdatedDate)
    {
        _lastUpdatedDate = lastUpdatedDate;
        return this;
    }

    internal AddressBuilder SetLastUpdatedBy(Guid? lastUpdatedBy)
    {
        if (lastUpdatedBy.HasValue)
        {
            Guard.Against.Default(lastUpdatedBy.Value, nameof(lastUpdatedBy));
        }

        _lastUpdatedBy = lastUpdatedBy;
        return this;
    }

    internal AddressBuilder SetDeletedDate(DateTime? deletedDate)
    {
        _deletedDate = deletedDate;
        return this;
    }

    internal AddressBuilder SetDeletedBy(Guid? deletedBy)
    {
        if (deletedBy.HasValue)
        {
            Guard.Against.Default(deletedBy.Value, nameof(deletedBy));
        }

        _deletedBy = deletedBy;
        return this;
    }

    internal AddressBuilder SetIsDeleted(bool isDeleted)
    {
        _isDeleted = isDeleted;
        return this;
    }

    internal AddressBuilder SetIsSystem(bool isSystem)
    {
        _isSystem = isSystem;
        return this;
    }

    internal AddressBuilder SetIsGdpr(bool isGdpr)
    {
        _isGdpr = isGdpr;
        return this;
    }

    internal Address Build()
    {
        return new Address(
            _id,
            _addressTypeId,
            _objectId,
            _co,
            _street1,
            _street2,
            _postalCode,
            _city,
            _state,
            _country,
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

internal interface IAddressWithCreateValues
{
    AddressBuilder WithCreateValues(EditAddressRequest request, Guid? userId);
}

internal interface IAddressWithUpdateValues
{
    AddressBuilder WithUpdateValues(Address existing, EditAddressRequest request, Guid? userId);
}
