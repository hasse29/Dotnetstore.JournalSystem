using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.WorkGroups;

public sealed class WorkGroupMappersTests
{
    [Fact]
    public void ToWorkGroupResponseMapsAllValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var workGroup = WorkGroupTestData.Create(id, "Administrator", "Can do everything");

        // Act
        var response = workGroup.ToWorkGroupResponse();

        // Assert
        response.Id.ShouldBe(id);
        response.Name.ShouldBe("Administrator");
        response.Description.ShouldBe("Can do everything");
    }

    [Fact]
    public void ToWorkGroupResponseKeepsEmptyDescription()
    {
        // Arrange
        var workGroup = WorkGroupTestData.Create(Guid.NewGuid(), "Administrator", string.Empty);

        // Act
        var response = workGroup.ToWorkGroupResponse();

        // Assert
        response.Description.ShouldBeEmpty();
    }
}
