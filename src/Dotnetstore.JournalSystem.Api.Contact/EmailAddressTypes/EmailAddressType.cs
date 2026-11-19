using Dotnetstore.BaseModel.Entities;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;

internal sealed class EmailAddressType : Auditable
{
    internal EmailAddressType(
        Guid id,
        string name,
        string description,
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
        Name = name;
        Description = description;
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

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
