using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal static class UserMappers
{
    internal static UserResponse ToUserResponse(this User user, string? socialSecurityNumberMasked = null)
    {
        return new UserResponse(
            user.Id,
            user.FirstName,
            user.MiddleName,
            user.LastName,
            user.Username,
            user.OwnCompanyId,
            user.RoleId,
            user.AuthProvider,
            user.IsLockedOut,
            user.LastLoginAt,
            DateOfBirth: ToDateOnly(user.DateOfBirth),
            IsMale: user.IsMale,
            SocialSecurityNumberMasked: socialSecurityNumberMasked);
    }

    internal static MyProfileResponse ToMyProfileResponse(
        this User user,
        string ownCompanyName,
        string roleName,
        string? socialSecurityNumberMasked,
        RoleAccess? access = null)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new MyProfileResponse(
            user.Id,
            user.FirstName,
            user.MiddleName,
            user.LastName,
            user.Username,
            user.AuthProvider,
            ToDateOnly(user.DateOfBirth),
            user.IsMale,
            socialSecurityNumberMasked,
            user.OwnCompanyId,
            ownCompanyName,
            user.RoleId,
            roleName,
            user.LastLoginAt,
            access?.EffectivePermissions ?? [],
            access?.IsAdministrator ?? false);
    }

    private static DateOnly? ToDateOnly(DateTime? value) => value is { } date ? DateOnly.FromDateTime(date) : null;
}
