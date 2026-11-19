using Dotnetstore.BaseModel.Entities;

namespace Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;

internal sealed class PhoneNumber : Auditable
{
    internal PhoneNumber(
        Guid id,
        Guid phoneNumberTypeId,
        Guid objectId,
        string number,
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
        PhoneNumberTypeId = phoneNumberTypeId;
        ObjectId = objectId;
        Number = number;
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

    public Guid PhoneNumberTypeId { get; set; }

    public Guid ObjectId { get; set; }

    public string Number { get; set; }
}
