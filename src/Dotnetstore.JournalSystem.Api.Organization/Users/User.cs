using System.Diagnostics.CodeAnalysis;
using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal sealed class User : PersonIdentity
{
    [SetsRequiredMembers]
    internal User(
        Guid id,
        string firstName,
        string? middleName,
        string lastName,
        string username,
        string passwordHash,
        string securityStamp,
        Guid ownCompanyId,
        Guid roleId,
        AuthProvider authProvider,
        string? externalId,
        bool accountIsApproved,
        bool isLockedOut,
        DateTime? lockoutEnd,
        int accessFailedCount,
        DateTime? lastLoginAt,
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
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        Username = username;
        PasswordHash = passwordHash;
        SecurityStamp = securityStamp;
        OwnCompanyId = ownCompanyId;
        RoleId = roleId;
        AuthProvider = authProvider;
        ExternalId = externalId;
        AccountIsApproved = accountIsApproved;
        IsLockedOut = isLockedOut;
        LockoutEnd = lockoutEnd;
        AccessFailedCount = accessFailedCount;
        LastLoginAt = lastLoginAt;
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

    public Guid OwnCompanyId { get; set; }

    public Guid RoleId { get; set; }

    public AuthProvider AuthProvider { get; set; }

    public string? ExternalId { get; set; }
}
