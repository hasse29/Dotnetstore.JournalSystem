using Ardalis.GuardClauses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.WorkGroups;

internal sealed class WorkGroupBuilder :
    IWorkGroupWithCreateValues,
    IWorkGroupWithUpdateValues
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

    private WorkGroupBuilder() { }

    internal static IWorkGroupWithCreateValues Create()
    {
        return new WorkGroupBuilder();
    }

    internal static IWorkGroupWithUpdateValues Update()
    {
        return new WorkGroupBuilder();
    }

    internal static WorkGroupBuilder Delete()
    {
        return new WorkGroupBuilder();
    }

    WorkGroupBuilder IWorkGroupWithCreateValues.WithCreateValues(EditWorkGroupRequest request, Guid? userId)
    {
        SetId(Guid.CreateVersion7());
        SetName(request.Name);
        SetDescription(request.Description);
        SetCreatedDate(DateTime.UtcNow);
        SetCreatedBy(userId);
        return this;
    }

    WorkGroupBuilder IWorkGroupWithUpdateValues.WithUpdateValues(WorkGroup existing, EditWorkGroupRequest request, Guid? userId)
    {
        SetWorkGroupValues(existing);
        SetName(request.Name);
        SetDescription(request.Description);
        SetLastUpdatedDate(DateTime.UtcNow);
        SetLastUpdatedBy(userId);
        return this;
    }

    private void SetWorkGroupValues(WorkGroup workGroup)
    {
        SetId(workGroup.Id);
        SetName(workGroup.Name);
        SetDescription(workGroup.Description);
        SetCreatedDate(workGroup.CreatedDate);
        SetCreatedBy(workGroup.CreatedBy);
        SetLastUpdatedDate(workGroup.LastUpdatedDate);
        SetLastUpdatedBy(workGroup.LastUpdatedBy);
        SetDeletedDate(workGroup.DeletedDate);
        SetDeletedBy(workGroup.DeletedBy);
        SetIsDeleted(workGroup.IsDeleted);
        SetIsSystem(workGroup.IsSystem);
        SetIsGdpr(workGroup.IsGdpr);
    }

    internal WorkGroupBuilder SetId(Guid id)
    {
        Guard.Against.Default(id);
        _id = id;
        return this;
    }

    internal WorkGroupBuilder SetName(string name)
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.StringTooLong(name, DataSchemeConstants.WorkGroupNameMaxLength);

        _name = name;
        return this;
    }

    internal WorkGroupBuilder SetDescription(string description)
    {
        Guard.Against.Null(description);
        Guard.Against.StringTooLong(description, DataSchemeConstants.WorkGroupDescriptionMaxLength);

        _description = description;
        return this;
    }

    internal WorkGroupBuilder SetCreatedDate(DateTime createdDate)
    {
        Guard.Against.Default(createdDate);

        _createdDate = createdDate;
        return this;
    }

    internal WorkGroupBuilder SetCreatedBy(Guid? createdBy)
    {
        if (createdBy.HasValue)
        {
            Guard.Against.Default(createdBy.Value, nameof(createdBy));
        }

        _createdBy = createdBy;
        return this;
    }

    internal WorkGroupBuilder SetLastUpdatedDate(DateTime? lastUpdatedDate)
    {
        _lastUpdatedDate = lastUpdatedDate;
        return this;
    }

    internal WorkGroupBuilder SetLastUpdatedBy(Guid? lastUpdatedBy)
    {
        if (lastUpdatedBy.HasValue)
        {
            Guard.Against.Default(lastUpdatedBy.Value, nameof(lastUpdatedBy));
        }

        _lastUpdatedBy = lastUpdatedBy;
        return this;
    }

    internal WorkGroupBuilder SetDeletedDate(DateTime? deletedDate)
    {
        _deletedDate = deletedDate;
        return this;
    }

    internal WorkGroupBuilder SetDeletedBy(Guid? deletedBy)
    {
        if (deletedBy.HasValue)
        {
            Guard.Against.Default(deletedBy.Value, nameof(deletedBy));
        }

        _deletedBy = deletedBy;
        return this;
    }

    internal WorkGroupBuilder SetIsDeleted(bool isDeleted)
    {
        _isDeleted = isDeleted;
        return this;
    }

    internal WorkGroupBuilder SetIsSystem(bool isSystem)
    {
        _isSystem = isSystem;
        return this;
    }

    internal WorkGroupBuilder SetIsGdpr(bool isGdpr)
    {
        _isGdpr = isGdpr;
        return this;
    }

    internal WorkGroup Build()
    {
        return new WorkGroup(
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

internal interface IWorkGroupWithCreateValues
{
    WorkGroupBuilder WithCreateValues(EditWorkGroupRequest request, Guid? userId);
}

internal interface IWorkGroupWithUpdateValues
{
    WorkGroupBuilder WithUpdateValues(WorkGroup existing, EditWorkGroupRequest request, Guid? userId);
}
