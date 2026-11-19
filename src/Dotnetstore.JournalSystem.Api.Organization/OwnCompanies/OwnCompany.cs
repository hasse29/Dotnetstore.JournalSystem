using Dotnetstore.BaseModel.Entities;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal sealed class OwnCompany : Company
{
    internal OwnCompany(
        Guid id,
        string name,
        string? description,
        Uri? logoUrl,
        Uri? websiteUrl,
        string? corporateNumber,
        string? taxId,
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
        LogoUrl = logoUrl;
        WebsiteUrl = websiteUrl;
        CorporateNumber = corporateNumber;
        TaxId = taxId;
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
}
