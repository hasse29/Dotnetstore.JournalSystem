using System.Security.Claims;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Fixtures;

internal sealed class FakeCurrentUserProfile(MyProfileResponse? profile) : ICurrentUserProfile
{
    internal static FakeCurrentUserProfile Administrator() => new(Profile(true, []));

    internal static FakeCurrentUserProfile With(params string[] permissions) => new(Profile(false, permissions));

    /// <summary>As when the API cannot be asked who is signed in.</summary>
    internal static FakeCurrentUserProfile Unknown() => new((MyProfileResponse?)null);

    public Task<MyProfileResponse?> GetAsync(ClaimsPrincipal principal, CancellationToken cancellationToken) =>
        Task.FromResult(profile);

    internal static MyProfileResponse Profile(bool isAdministrator, IReadOnlyList<string> permissions) => new(
        Guid.NewGuid(),
        "Test",
        null,
        "Person",
        "test@example.com",
        AuthProvider.Local,
        null,
        false,
        null,
        Guid.NewGuid(),
        "Test AB",
        Guid.NewGuid(),
        isAdministrator ? RoleNames.Administrator : "Nurse",
        null,
        permissions,
        isAdministrator);
}
