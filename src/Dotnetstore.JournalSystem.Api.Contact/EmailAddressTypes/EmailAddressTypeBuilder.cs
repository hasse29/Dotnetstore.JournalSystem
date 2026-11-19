using Ardalis.GuardClauses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;

namespace Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;

internal sealed class EmailAddressTypeBuilder :
    IEmailAddressTypeWithCreateValues,
    IEmailAddressTypeWithUpdateValues
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

    private EmailAddressTypeBuilder() { }

    internal static IEmailAddressTypeWithCreateValues Create()
    {
        return new EmailAddressTypeBuilder();
    }

    internal static IEmailAddressTypeWithUpdateValues Update()
    {
        return new EmailAddressTypeBuilder();
    }

    internal static EmailAddressTypeBuilder Delete()
    {
        return new EmailAddressTypeBuilder();
    }

    EmailAddressTypeBuilder IEmailAddressTypeWithCreateValues.WithCreateValues(EditEmailAddressTypeRequest request, Guid? userId)
    {
        SetId(Guid.CreateVersion7());
        SetName(request.Name);
        SetDescription(request.Description);
        SetCreatedDate(DateTime.UtcNow);
        SetCreatedBy(userId);
        return this;
    }

    EmailAddressTypeBuilder IEmailAddressTypeWithUpdateValues.WithUpdateValues(EmailAddressType existing, EditEmailAddressTypeRequest request, Guid? userId)
    {
        SetEmailAddressTypeValues(existing);
        SetName(request.Name);
        SetDescription(request.Description);
        SetLastUpdatedDate(DateTime.UtcNow);
        SetLastUpdatedBy(userId);
        return this;
    }

    private void SetEmailAddressTypeValues(EmailAddressType emailAddressType)
    {
        SetId(emailAddressType.Id);
        SetName(emailAddressType.Name);
        SetDescription(emailAddressType.Description);
        SetCreatedDate(emailAddressType.CreatedDate);
        SetCreatedBy(emailAddressType.CreatedBy);
        SetLastUpdatedDate(emailAddressType.LastUpdatedDate);
        SetLastUpdatedBy(emailAddressType.LastUpdatedBy);
        SetDeletedDate(emailAddressType.DeletedDate);
        SetDeletedBy(emailAddressType.DeletedBy);
        SetIsDeleted(emailAddressType.IsDeleted);
        SetIsSystem(emailAddressType.IsSystem);
        SetIsGdpr(emailAddressType.IsGdpr);
    }

    internal EmailAddressTypeBuilder SetId(Guid id)
    {
        Guard.Against.Default(id);
        _id = id;
        return this;
    }

    internal EmailAddressTypeBuilder SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.StringTooLong(name, DataSchemeConstants.EmailAddressTypeNameMaxLength);

        _name = name;
        return this;
    }

    internal EmailAddressTypeBuilder SetDescription(string description)
    {
        Guard.Against.Null(description);
        Guard.Against.StringTooLong(description, DataSchemeConstants.EmailAddressTypeDescriptionMaxLength);

        _description = description;
        return this;
    }

    internal EmailAddressTypeBuilder SetCreatedDate(DateTime createdDate)
    {
        Guard.Against.Default(createdDate);

        _createdDate = createdDate;
        return this;
    }

    internal EmailAddressTypeBuilder SetCreatedBy(Guid? createdBy)
    {
        if (createdBy.HasValue)
        {
            Guard.Against.Default(createdBy.Value, nameof(createdBy));
        }

        _createdBy = createdBy;
        return this;
    }

    internal EmailAddressTypeBuilder SetLastUpdatedDate(DateTime? lastUpdatedDate)
    {
        _lastUpdatedDate = lastUpdatedDate;
        return this;
    }

    internal EmailAddressTypeBuilder SetLastUpdatedBy(Guid? lastUpdatedBy)
    {
        if (lastUpdatedBy.HasValue)
        {
            Guard.Against.Default(lastUpdatedBy.Value, nameof(lastUpdatedBy));
        }

        _lastUpdatedBy = lastUpdatedBy;
        return this;
    }

    internal EmailAddressTypeBuilder SetDeletedDate(DateTime? deletedDate)
    {
        _deletedDate = deletedDate;
        return this;
    }

    internal EmailAddressTypeBuilder SetDeletedBy(Guid? deletedBy)
    {
        if (deletedBy.HasValue)
        {
            Guard.Against.Default(deletedBy.Value, nameof(deletedBy));
        }

        _deletedBy = deletedBy;
        return this;
    }

    internal EmailAddressTypeBuilder SetIsDeleted(bool isDeleted)
    {
        _isDeleted = isDeleted;
        return this;
    }

    internal EmailAddressTypeBuilder SetIsSystem(bool isSystem)
    {
        _isSystem = isSystem;
        return this;
    }

    internal EmailAddressTypeBuilder SetIsGdpr(bool isGdpr)
    {
        _isGdpr = isGdpr;
        return this;
    }

    internal EmailAddressType Build()
    {
        return new EmailAddressType(
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

internal interface IEmailAddressTypeWithCreateValues
{
    EmailAddressTypeBuilder WithCreateValues(EditEmailAddressTypeRequest request, Guid? userId);
}

internal interface IEmailAddressTypeWithUpdateValues
{
    EmailAddressTypeBuilder WithUpdateValues(EmailAddressType existing, EditEmailAddressTypeRequest request, Guid? userId);
}
