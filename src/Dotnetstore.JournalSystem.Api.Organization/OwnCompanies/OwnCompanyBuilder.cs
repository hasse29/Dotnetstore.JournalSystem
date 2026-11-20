using Ardalis.GuardClauses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal sealed class OwnCompanyBuilder :
    IWithCreateValues,
    IWithUpdateValues
{
    private Guid _id;
    private string _name = string.Empty;
    private string? _description;
    private Uri? _logoUrl;
    private Uri? _websiteUrl;
    private string? _corporateNumber;
    private string? _taxId;
    private DateTime _createdDate;
    private Guid? _createdBy;
    private DateTime? _lastUpdatedDate;
    private Guid? _lastUpdatedBy;
    private DateTime? _deletedDate;
    private Guid? _deletedBy;
    private bool _isDeleted;
    private bool _isSystem;
    private bool _isGdpr;

    private OwnCompanyBuilder() { }

    internal static IWithCreateValues Create()
    {
        return new OwnCompanyBuilder();
    }

    internal static IWithUpdateValues Update()
    {
        return new OwnCompanyBuilder();
    }

    internal static OwnCompanyBuilder Delete()
    {
        return new OwnCompanyBuilder();
    }

    OwnCompanyBuilder IWithCreateValues.WithCreateValues(EditOwnCompanyRequest request, Guid? userId)
    {
        SetId(Guid.CreateVersion7());
        SetName(request.Name);
        SetDescription(request.Description);
        SetLogoUrl(request.LogoUrl);
        SetWebsiteUrl(request.WebsiteUrl);
        SetCorporateNumber(request.CorporateNumber);
        SetTaxId(request.TaxId);
        SetCreatedDate(DateTime.UtcNow);
        SetCreatedBy(userId);
        return this;
    }

    OwnCompanyBuilder IWithUpdateValues.WithUpdateValues(OwnCompany existing, EditOwnCompanyRequest request, Guid? userId)
    {
        SetOwnCompanyValues(existing);
        SetName(request.Name);
        SetDescription(request.Description);
        SetLogoUrl(request.LogoUrl);
        SetWebsiteUrl(request.WebsiteUrl);
        SetCorporateNumber(request.CorporateNumber);
        SetTaxId(request.TaxId);
        SetLastUpdatedDate(DateTime.UtcNow);
        SetLastUpdatedBy(userId);
        return this;
    }

    private void SetOwnCompanyValues(OwnCompany ownCompany)
    {
        SetId(ownCompany.Id);
        SetName(ownCompany.Name);
        SetDescription(ownCompany.Description);
        SetLogoUrl(ownCompany.LogoUrl);
        SetWebsiteUrl(ownCompany.WebsiteUrl);
        SetCorporateNumber(ownCompany.CorporateNumber);
        SetTaxId(ownCompany.TaxId);
        SetCreatedDate(ownCompany.CreatedDate);
        SetCreatedBy(ownCompany.CreatedBy);
        SetLastUpdatedDate(ownCompany.LastUpdatedDate);
        SetLastUpdatedBy(ownCompany.LastUpdatedBy);
        SetDeletedDate(ownCompany.DeletedDate);
        SetDeletedBy(ownCompany.DeletedBy);
        SetIsDeleted(ownCompany.IsDeleted);
        SetIsSystem(ownCompany.IsSystem);
        SetIsGdpr(ownCompany.IsGdpr);
    }

    internal OwnCompanyBuilder SetId(Guid id)
    {
        Guard.Against.Default(id);
        _id = id;
        return this;
    }

    internal OwnCompanyBuilder SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.StringTooLong(name, DataSchemeConstants.OwnCompanyNameMaxLength);

        _name = name;
        return this;
    }

    internal OwnCompanyBuilder SetDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description))
        {
            Guard.Against.StringTooLong(description, DataSchemeConstants.OwnCompanyDescriptionMaxLength);
        }

        _description = description;
        return this;
    }

    internal OwnCompanyBuilder SetLogoUrl(Uri? logoUrl)
    {
        _logoUrl = logoUrl;
        return this;
    }

    internal OwnCompanyBuilder SetWebsiteUrl(Uri? websiteUrl)
    {
        _websiteUrl = websiteUrl;
        return this;
    }

    internal OwnCompanyBuilder SetCorporateNumber(string? corporateNumber)
    {
        if (!string.IsNullOrWhiteSpace(corporateNumber))
        {
            Guard.Against.StringTooLong(corporateNumber, DataSchemeConstants.OwnCompanyCorporateNumberMaxLength);
        }

        _corporateNumber = corporateNumber;
        return this;
    }

    internal OwnCompanyBuilder SetTaxId(string? taxId)
    {
        if (!string.IsNullOrWhiteSpace(taxId))
        {
            Guard.Against.StringTooLong(taxId, DataSchemeConstants.OwnCompanyTaxIdMaxLength);
        }

        _taxId = taxId;
        return this;
    }

    internal OwnCompanyBuilder SetCreatedDate(DateTime createdDate)
    {
        Guard.Against.Default(createdDate);

        _createdDate = createdDate;
        return this;
    }

    internal OwnCompanyBuilder SetCreatedBy(Guid? createdBy)
    {
        if (createdBy.HasValue)
        {
            Guard.Against.Default(createdBy.Value, nameof(createdBy));
        }

        _createdBy = createdBy;
        return this;
    }

    internal OwnCompanyBuilder SetLastUpdatedDate(DateTime? lastUpdatedDate)
    {
        _lastUpdatedDate = lastUpdatedDate;
        return this;
    }

    internal OwnCompanyBuilder SetLastUpdatedBy(Guid? lastUpdatedBy)
    {
        if (lastUpdatedBy.HasValue)
        {
            Guard.Against.Default(lastUpdatedBy.Value, nameof(lastUpdatedBy));
        }

        _lastUpdatedBy = lastUpdatedBy;
        return this;
    }

    internal OwnCompanyBuilder SetDeletedDate(DateTime? deletedDate)
    {
        _deletedDate = deletedDate;
        return this;
    }

    internal OwnCompanyBuilder SetDeletedBy(Guid? deletedBy)
    {
        if (deletedBy.HasValue)
        {
            Guard.Against.Default(deletedBy.Value, nameof(deletedBy));
        }

        _deletedBy = deletedBy;
        return this;
    }

    internal OwnCompanyBuilder SetIsDeleted(bool isDeleted)
    {
        _isDeleted = isDeleted;
        return this;
    }

    internal OwnCompanyBuilder SetIsSystem(bool isSystem)
    {
        _isSystem = isSystem;
        return this;
    }

    internal OwnCompanyBuilder SetIsGdpr(bool isGdpr)
    {
        _isGdpr = isGdpr;
        return this;
    }

    internal OwnCompany Build()
    {
        return new OwnCompany(
            _id,
            _name,
            _description,
            _logoUrl,
            _websiteUrl,
            _corporateNumber,
            _taxId,
            _createdDate,
            _createdBy,
            _lastUpdatedDate,
            _lastUpdatedBy,
            _deletedDate,
            _deletedBy,
            _isDeleted,
            _isSystem,
            _isGdpr)
        {
            Name = _name
        };
    }
}

internal interface IWithCreateValues
{
    OwnCompanyBuilder WithCreateValues(EditOwnCompanyRequest request, Guid? userId);
}

internal interface IWithUpdateValues
{
    OwnCompanyBuilder WithUpdateValues(OwnCompany existing, EditOwnCompanyRequest request, Guid? userId);
}