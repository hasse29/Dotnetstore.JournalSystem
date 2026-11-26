using Dotnetstore.JournalSystem.Shared.Constants;
using Shouldly;

namespace Dotnetstore.JournalSystem.Shared.Tests.Constants;

public sealed class PermissionsTests
{
    [Fact]
    public void EveryRightIsDistinctAndFitsTheColumn()
    {
        // Act & Assert
        Permissions.All.ShouldBeUnique();
        Permissions.All.ShouldAllBe(x => x.Length <= DataSchemeConstants.PermissionMaxLength);
    }

    [Fact]
    public void RolesCanOnlyBeViewed()
    {
        // Act
        var roleRights = Permissions.All.Where(x => x.StartsWith("roles:", StringComparison.Ordinal));

        // Assert
        roleRights.ShouldBe([Permissions.Roles.View]);
    }

    [Theory]
    [InlineData(Permissions.WorkGroups.Create, Permissions.WorkGroups.View)]
    [InlineData(Permissions.WorkGroups.Delete, Permissions.WorkGroups.View)]
    [InlineData(Permissions.Users.View, Permissions.Roles.View)]
    [InlineData(Permissions.Users.Edit, Permissions.OwnCompanies.View)]
    [InlineData(Permissions.Users.Delete, Permissions.Roles.View)]
    [InlineData(Permissions.AccessRequests.Edit, Permissions.OwnCompanies.View)]
    [InlineData(Permissions.ContactDetails.Edit, Permissions.ContactTypes.View)]
    public void WithRequiredAddsWhatARightIsUselessWithout(string given, string expected)
    {
        // Act
        var result = Permissions.WithRequired([given]);

        // Assert
        result.ShouldContain(given);
        result.ShouldContain(expected);
    }

    [Fact]
    public void WithRequiredAddsNoRightToChangeAnything()
    {
        // Act
        var result = Permissions.WithRequired([Permissions.Users.View]);

        // Assert
        result.ShouldBe([Permissions.Users.View, Permissions.Roles.View]);
    }

    [Fact]
    public void WithRequiredOfNothingIsNothing()
    {
        // Act & Assert
        Permissions.WithRequired([]).ShouldBeEmpty();
    }

    [Fact]
    public void WithRequiredDropsUnknownAndRepeatedValues()
    {
        // Act
        var result = Permissions.WithRequired([Permissions.Roles.View, Permissions.Roles.View, "everything:all"]);

        // Assert
        result.ShouldBe([Permissions.Roles.View]);
    }
}
