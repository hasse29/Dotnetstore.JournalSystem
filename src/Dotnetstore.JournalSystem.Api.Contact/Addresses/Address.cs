using Dotnetstore.BaseModel.Entities;

namespace Dotnetstore.JournalSystem.Api.Contact.Addresses;

internal sealed class Address : Auditable
{
    // The parameter is named cO because EF Core binds constructor parameters by camel-casing the property name.
    internal Address(
        Guid id,
        Guid addressTypeId,
        Guid objectId,
        string? cO,
        string street1,
        string? street2,
        string postalCode,
        string city,
        string state,
        string country,
        DateTime createdDate,
        Guid? createdBy,
        DateTime? lastUpdatedDate,
        Guid? lastUpdatedBy,
        DateTime? deletedDate,
        Guid? deletedBy,
        bool isDeleted,
        bool isSystem,
        bool isGdpr)
    {
        Id = id;
        AddressTypeId = addressTypeId;
        ObjectId = objectId;
        CO = cO;
        Street1 = street1;
        Street2 = street2;
        PostalCode = postalCode;
        City = city;
        State = state;
        Country = country;
        CreatedDate = createdDate;
        CreatedBy = createdBy;
        LastUpdatedDate = lastUpdatedDate;
        LastUpdatedBy = lastUpdatedBy;
        DeletedDate = deletedDate;
        DeletedBy = deletedBy;
        IsDeleted = isDeleted;
        IsSystem = isSystem;
        IsGdpr = isGdpr;
    }

    public Guid Id { get; set; }

    public Guid AddressTypeId { get; set; }

    public Guid ObjectId { get; set; }

    public string? CO { get; set; }

    public string Street1 { get; set; } = string.Empty;

    public string? Street2 { get; set; }

    public string PostalCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
}
