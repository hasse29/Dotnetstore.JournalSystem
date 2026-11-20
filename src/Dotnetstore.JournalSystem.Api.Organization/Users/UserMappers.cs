using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal static class UserMappers
{
    internal static UserResponse ToUserResponse(this User user)
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
            user.LastLoginAt);
    }
}
