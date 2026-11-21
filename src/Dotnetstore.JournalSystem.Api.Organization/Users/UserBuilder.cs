using Ardalis.GuardClauses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal sealed class UserBuilder :
    IUserWithCreateValues,
    IUserWithUpdateValues
{
    private Guid _id;
    private string _firstName = string.Empty;
    private string? _middleName;
    private string _lastName = string.Empty;
    private DateTime? _dateOfBirth;
    private string? _socialSecurityNumber;
    private bool _isMale;
    private string _username = string.Empty;
    private string _passwordHash = string.Empty;
    private string _securityStamp = string.Empty;
    private Guid _ownCompanyId;
    private Guid _roleId;
    private AuthProvider _authProvider;
    private string? _externalId;
    private bool _accountIsApproved;
    private bool _isLockedOut;
    private DateTime? _lockoutEnd;
    private int _accessFailedCount;
    private DateTime? _lastLoginAt;
    private DateTime _createdDate;
    private Guid? _createdBy;
    private DateTime? _lastUpdatedDate;
    private Guid? _lastUpdatedBy;
    private DateTime? _deletedDate;
    private Guid? _deletedBy;
    private bool _isDeleted;
    private bool _isSystem;
    private bool _isGdpr = true;

    private UserBuilder() { }

    internal static IUserWithCreateValues Create()
    {
        return new UserBuilder();
    }

    internal static IUserWithUpdateValues Update()
    {
        return new UserBuilder();
    }

    UserBuilder IUserWithCreateValues.WithCreateValues(
        CreateUserRequest request,
        string passwordHash,
        string? externalId,
        Guid? userId)
    {
        SetId(Guid.CreateVersion7());
        SetFirstName(request.FirstName);
        SetMiddleName(request.MiddleName);
        SetLastName(request.LastName);
        SetDateOfBirth(request.DateOfBirth);
        _isMale = request.IsMale ?? false;
        SetUsername(request.Username);
        SetPasswordHash(passwordHash);
        SetSecurityStamp(Guid.NewGuid().ToString("N"));
        SetOwnCompanyId(request.OwnCompanyId);
        SetRoleId(request.RoleId);
        SetAuthProvider(request.AuthProvider);
        SetExternalId(externalId);
        SetAccountIsApproved(true);
        SetCreatedDate(DateTime.UtcNow);
        SetCreatedBy(userId);
        return this;
    }

    UserBuilder IUserWithUpdateValues.WithUpdateValues(User existing, EditUserRequest request, Guid? userId)
    {
        SetUserValues(existing);
        SetFirstName(request.FirstName);
        SetMiddleName(request.MiddleName);
        SetLastName(request.LastName);
        if (request.DateOfBirth is { } dateOfBirth)
        {
            SetDateOfBirth(dateOfBirth);
        }

        if (request.IsMale is { } isMale)
        {
            _isMale = isMale;
        }

        SetOwnCompanyId(request.OwnCompanyId);
        SetRoleId(request.RoleId);
        SetLastUpdatedDate(DateTime.UtcNow);
        SetLastUpdatedBy(userId);
        return this;
    }

    private void SetUserValues(User user)
    {
        SetId(user.Id);
        SetFirstName(user.FirstName);
        SetMiddleName(user.MiddleName);
        SetLastName(user.LastName);
        _dateOfBirth = user.DateOfBirth;
        _socialSecurityNumber = user.SocialSecurityNumber;
        _isMale = user.IsMale;
        SetUsername(user.Username);
        _passwordHash = user.PasswordHash;
        SetSecurityStamp(user.SecurityStamp);
        SetOwnCompanyId(user.OwnCompanyId);
        SetRoleId(user.RoleId);
        SetAuthProvider(user.AuthProvider);
        SetExternalId(user.ExternalId);
        SetAccountIsApproved(user.AccountIsApproved);
        _isLockedOut = user.IsLockedOut;
        _lockoutEnd = user.LockoutEnd;
        _accessFailedCount = user.AccessFailedCount;
        _lastLoginAt = user.LastLoginAt;
        SetCreatedDate(user.CreatedDate);
        SetCreatedBy(user.CreatedBy);
        SetLastUpdatedDate(user.LastUpdatedDate);
        SetLastUpdatedBy(user.LastUpdatedBy);
        _deletedDate = user.DeletedDate;
        _deletedBy = user.DeletedBy;
        _isDeleted = user.IsDeleted;
        _isSystem = user.IsSystem;
        _isGdpr = user.IsGdpr;
    }

    internal UserBuilder SetId(Guid id)
    {
        Guard.Against.Default(id);
        _id = id;
        return this;
    }

    internal UserBuilder SetFirstName(string firstName)
    {
        Guard.Against.NullOrWhiteSpace(firstName);
        Guard.Against.StringTooLong(firstName, DataSchemeConstants.UserNameMaxLength);

        _firstName = firstName;
        return this;
    }

    internal UserBuilder SetMiddleName(string? middleName)
    {
        if (middleName is not null)
        {
            Guard.Against.StringTooLong(middleName, DataSchemeConstants.UserNameMaxLength);
        }

        _middleName = middleName;
        return this;
    }

    internal UserBuilder SetLastName(string lastName)
    {
        Guard.Against.NullOrWhiteSpace(lastName);
        Guard.Against.StringTooLong(lastName, DataSchemeConstants.UserNameMaxLength);

        _lastName = lastName;
        return this;
    }

    internal UserBuilder SetDateOfBirth(DateOnly? dateOfBirth)
    {
        _dateOfBirth = dateOfBirth?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return this;
    }

    // The number is encrypted against this user's id, so the id must be set first.
    internal UserBuilder SetPersonalIdentityNumber(string? personalIdentityNumber, IPersonalIdentityNumberProtector protector)
    {
        ArgumentNullException.ThrowIfNull(protector);

        _socialSecurityNumber = protector.Protect(personalIdentityNumber, _id);
        return this;
    }

    internal UserBuilder SetIsMale(bool isMale)
    {
        _isMale = isMale;
        return this;
    }

    internal UserBuilder SetUsername(string username)
    {
        Guard.Against.NullOrWhiteSpace(username);
        Guard.Against.StringTooLong(username, DataSchemeConstants.UserUsernameMaxLength);

        _username = username;
        return this;
    }

    internal UserBuilder SetPasswordHash(string passwordHash)
    {
        Guard.Against.Null(passwordHash);

        _passwordHash = passwordHash;
        return this;
    }

    internal UserBuilder SetSecurityStamp(string securityStamp)
    {
        Guard.Against.NullOrWhiteSpace(securityStamp);

        _securityStamp = securityStamp;
        return this;
    }

    internal UserBuilder SetOwnCompanyId(Guid ownCompanyId)
    {
        Guard.Against.Default(ownCompanyId);

        _ownCompanyId = ownCompanyId;
        return this;
    }

    internal UserBuilder SetRoleId(Guid roleId)
    {
        Guard.Against.Default(roleId);

        _roleId = roleId;
        return this;
    }

    internal UserBuilder SetAuthProvider(AuthProvider authProvider)
    {
        Guard.Against.EnumOutOfRange(authProvider);

        _authProvider = authProvider;
        return this;
    }

    internal UserBuilder SetExternalId(string? externalId)
    {
        if (externalId is not null)
        {
            Guard.Against.NullOrWhiteSpace(externalId);
            Guard.Against.StringTooLong(externalId, DataSchemeConstants.UserExternalIdMaxLength);
        }

        _externalId = externalId;
        return this;
    }

    internal UserBuilder SetAccountIsApproved(bool accountIsApproved)
    {
        _accountIsApproved = accountIsApproved;
        return this;
    }

    internal UserBuilder SetIsSystem(bool isSystem)
    {
        _isSystem = isSystem;
        return this;
    }

    internal UserBuilder SetCreatedDate(DateTime createdDate)
    {
        Guard.Against.Default(createdDate);

        _createdDate = createdDate;
        return this;
    }

    internal UserBuilder SetCreatedBy(Guid? createdBy)
    {
        if (createdBy.HasValue)
        {
            Guard.Against.Default(createdBy.Value, nameof(createdBy));
        }

        _createdBy = createdBy;
        return this;
    }

    internal UserBuilder SetLastUpdatedDate(DateTime? lastUpdatedDate)
    {
        _lastUpdatedDate = lastUpdatedDate;
        return this;
    }

    internal UserBuilder SetLastUpdatedBy(Guid? lastUpdatedBy)
    {
        if (lastUpdatedBy.HasValue)
        {
            Guard.Against.Default(lastUpdatedBy.Value, nameof(lastUpdatedBy));
        }

        _lastUpdatedBy = lastUpdatedBy;
        return this;
    }

    internal User Build()
    {
        return new User(
            _id,
            _firstName,
            _middleName,
            _lastName,
            _username,
            _passwordHash,
            _securityStamp,
            _ownCompanyId,
            _roleId,
            _authProvider,
            _externalId,
            _accountIsApproved,
            _isLockedOut,
            _lockoutEnd,
            _accessFailedCount,
            _lastLoginAt,
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
            DateOfBirth = _dateOfBirth,
            SocialSecurityNumber = _socialSecurityNumber,
            IsMale = _isMale
        };
    }
}

internal interface IUserWithCreateValues
{
    UserBuilder WithCreateValues(CreateUserRequest request, string passwordHash, string? externalId, Guid? userId);
}

internal interface IUserWithUpdateValues
{
    UserBuilder WithUpdateValues(User existing, EditUserRequest request, Guid? userId);
}
