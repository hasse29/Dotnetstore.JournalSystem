using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

internal static class UserTestData
{
    internal static readonly DateTime CreatedDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    internal static CreateUserRequest CreateRequest(
        string username = "anna@example.com",
        AuthProvider authProvider = AuthProvider.Local,
        string? password = "a-long-enough-password",
        Guid? ownCompanyId = null,
        Guid? roleId = null)
    {
        return new CreateUserRequest(
            "Anna",
            null,
            "Svensson",
            username,
            ownCompanyId ?? Guid.NewGuid(),
            roleId ?? Guid.NewGuid(),
            authProvider,
            password);
    }

    internal static User Create(
        Guid id,
        string username = "anna@example.com",
        AuthProvider authProvider = AuthProvider.Local,
        string? externalId = null,
        bool isSystem = false)
    {
        return new User(
            id,
            "Anna",
            null,
            "Svensson",
            username,
            "hash",
            "stamp",
            Guid.NewGuid(),
            Guid.NewGuid(),
            authProvider,
            externalId,
            true,
            false,
            null,
            0,
            null,
            CreatedDate,
            null,
            null,
            null,
            null,
            null,
            false,
            isSystem,
            true);
    }
}
