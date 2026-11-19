using Ardalis.GuardClauses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Roles;

internal sealed class RoleBuilder :
    IRoleWithCreateValues,
    IRoleWithUpdateValues
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

    private RoleBuilder() { }

    internal static IRoleWithCreateValues Create()
    {
        return new RoleBuilder();
    }

    internal static IRoleWithUpdateValues Update()
    {
        return new RoleBuilder();
    }

    internal static RoleBuilder Delete()
    {
        return new RoleBuilder();
    }

    RoleBuilder IRoleWithCreateValues.WithCreateValues(EditRoleRequest request, Guid? userId)
    {
        SetId(Guid.CreateVersion7());
        SetName(request.Name);
        SetDescription(request.Description);
        SetCreatedDate(DateTime.UtcNow);
        SetCreatedBy(userId);
        return this;
    }

    RoleBuilder IRoleWithUpdateValues.WithUpdateValues(Role existing, EditRoleRequest request, Guid? userId)
    {
        SetRoleValues(existing);
        SetName(request.Name);
        SetDescription(request.Description);
        SetLastUpdatedDate(DateTime.UtcNow);
        SetLastUpdatedBy(userId);
        return this;
    }

    private void SetRoleValues(Role role)
    {
        SetId(role.Id);
        SetName(role.Name);
        SetDescription(role.Description);
        SetCreatedDate(role.CreatedDate);
        SetCreatedBy(role.CreatedBy);
        SetLastUpdatedDate(role.LastUpdatedDate);
        SetLastUpdatedBy(role.LastUpdatedBy);
        SetDeletedDate(role.DeletedDate);
        SetDeletedBy(role.DeletedBy);
        SetIsDeleted(role.IsDeleted);
        SetIsSystem(role.IsSystem);
        SetIsGdpr(role.IsGdpr);
    }

    internal RoleBuilder SetId(Guid id)
    {
        Guard.Against.Default(id);
        _id = id;
        return this;
    }

    internal RoleBuilder SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.StringTooLong(name, DataSchemeConstants.RoleNameMaxLength);

        _name = name;
        return this;
    }

    internal RoleBuilder SetDescription(string description)
    {
        Guard.Against.Null(description);
        Guard.Against.StringTooLong(description, DataSchemeConstants.RoleDescriptionMaxLength);

        _description = description;
        return this;
    }

    internal RoleBuilder SetCreatedDate(DateTime createdDate)
    {
        Guard.Against.Default(createdDate);

        _createdDate = createdDate;
        return this;
    }

    internal RoleBuilder SetCreatedBy(Guid? createdBy)
    {
        if (createdBy.HasValue)
        {
            Guard.Against.Default(createdBy.Value, nameof(createdBy));
        }

        _createdBy = createdBy;
        return this;
    }

    internal RoleBuilder SetLastUpdatedDate(DateTime? lastUpdatedDate)
    {
        _lastUpdatedDate = lastUpdatedDate;
        return this;
    }

    internal RoleBuilder SetLastUpdatedBy(Guid? lastUpdatedBy)
    {
        if (lastUpdatedBy.HasValue)
        {
            Guard.Against.Default(lastUpdatedBy.Value, nameof(lastUpdatedBy));
        }

        _lastUpdatedBy = lastUpdatedBy;
        return this;
    }

    internal RoleBuilder SetDeletedDate(DateTime? deletedDate)
    {
        _deletedDate = deletedDate;
        return this;
    }

    internal RoleBuilder SetDeletedBy(Guid? deletedBy)
    {
        if (deletedBy.HasValue)
        {
            Guard.Against.Default(deletedBy.Value, nameof(deletedBy));
        }

        _deletedBy = deletedBy;
        return this;
    }

    internal RoleBuilder SetIsDeleted(bool isDeleted)
    {
        _isDeleted = isDeleted;
        return this;
    }

    internal RoleBuilder SetIsSystem(bool isSystem)
    {
        _isSystem = isSystem;
        return this;
    }

    internal RoleBuilder SetIsGdpr(bool isGdpr)
    {
        _isGdpr = isGdpr;
        return this;
    }

    internal Role Build()
    {
        return new Role(
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

internal interface IRoleWithCreateValues
{
    RoleBuilder WithCreateValues(EditRoleRequest request, Guid? userId);
}

internal interface IRoleWithUpdateValues
{
    RoleBuilder WithUpdateValues(Role existing, EditRoleRequest request, Guid? userId);
}
