using System.Security.Claims;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class ActingUserTests
{
    private static ClaimsPrincipal Principal(params Claim[] claims) => new(new ClaimsIdentity(claims, "test"));

    [Fact]
    public void ReadsTheRightsFromTheClaims()
    {
        // Act
        var actor = ActingUser.From(Principal(
            new Claim(AuthClaimTypes.Role, "Nurse"),
            new Claim(AuthClaimTypes.Permission, Permissions.Users.View)));

        // Assert
        actor.IsAdministrator.ShouldBeFalse();
        actor.Permissions.ShouldBe([Permissions.Users.View]);
    }

    [Fact]
    public void RecognisesAnAdministratorByRole()
    {
        // Act
        var actor = ActingUser.From(Principal(new Claim(AuthClaimTypes.Role, RoleNames.Administrator)));

        // Assert
        actor.IsAdministrator.ShouldBeTrue();
    }
}
