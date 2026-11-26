using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;

public sealed class RoleMappersTests
{
    [Fact]
    public void ToRoleResponseMapsAllValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var role = RoleTestData.Create(id, "Administrator", "Can do everything");

        // Act
        var response = role.ToRoleResponse();

        // Assert
        response.Id.ShouldBe(id);
        response.Name.ShouldBe("Administrator");
        response.Description.ShouldBe("Can do everything");
    }

    [Fact]
    public void ToRoleResponseKeepsEmptyDescription()
    {
        // Arrange
        var role = RoleTestData.Create(Guid.NewGuid(), "Administrator", string.Empty);

        // Act
        var response = role.ToRoleResponse();

        // Assert
        response.Description.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ToRoleResponseTellsWhetherTheRoleIsASystemRole(bool isSystem)
    {
        // Arrange
        var role = RoleTestData.Create(Guid.NewGuid(), "Administrator", isSystem: isSystem);

        // Act
        var response = role.ToRoleResponse();

        // Assert
        response.IsSystem.ShouldBe(isSystem);
    }
}
