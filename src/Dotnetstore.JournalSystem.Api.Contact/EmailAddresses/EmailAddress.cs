using Dotnetstore.BaseModel.Entities;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;

internal sealed class EmailAddress : Auditable
{
    internal EmailAddress(
        Guid id,
        Guid emailAddressTypeId,
        Guid objectId,
        string address,
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
        EmailAddressTypeId = emailAddressTypeId;
        ObjectId = objectId;
        Address = address;
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

    public Guid EmailAddressTypeId { get; set; }

    public Guid ObjectId { get; set; }

    public string Address { get; set; }
}
