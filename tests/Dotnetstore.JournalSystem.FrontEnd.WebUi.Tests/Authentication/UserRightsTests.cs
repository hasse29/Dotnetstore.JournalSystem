using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Authentication;

public sealed class UserRightsTests
{
    [Fact]
    public void AnAdministratorHasEveryRight()
    {
        // Arrange
        var rights = UserRights.From(FakeCurrentUserProfile.Profile(true, []));

        // Act & Assert
        rights.IsAdministrator.ShouldBeTrue();
        Permissions.All.ShouldAllBe(x => rights.Has(x));
    }

    [Fact]
    public void OthersHaveOnlyTheRightsInTheirProfile()
    {
        // Arrange
        var rights = UserRights.From(FakeCurrentUserProfile.Profile(false, [Permissions.Users.View]));

        // Act & Assert
        rights.IsAdministrator.ShouldBeFalse();
        rights.Has(Permissions.Users.View).ShouldBeTrue();
        rights.Has(Permissions.Users.Edit).ShouldBeFalse();
    }

    [Fact]
    public void NoProfileMeansNoRights()
    {
        // Arrange
        var rights = UserRights.From(null);

        // Act & Assert
        rights.IsAdministrator.ShouldBeFalse();
        Permissions.All.ShouldAllBe(x => !rights.Has(x));
    }
}
