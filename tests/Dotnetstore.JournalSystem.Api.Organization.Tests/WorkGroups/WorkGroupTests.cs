using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.WorkGroups;

public sealed class WorkGroupTests
{
    private static readonly Guid WorkGroupId = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    [Fact]
    public void ConstructorSetsAllValues()
    {
        // Arrange & Act
        var workGroup = WorkGroupTestData.Create(WorkGroupId, "Administrator", "Can do everything");

        // Assert
        workGroup.Id.ShouldBe(WorkGroupId);
        workGroup.Name.ShouldBe("Administrator");
        workGroup.Description.ShouldBe("Can do everything");
        workGroup.CreatedDate.ShouldBe(WorkGroupTestData.CreatedDate);
    }

    [Fact]
    public void ConstructorSetsAuditValues()
    {
        // Arrange
        var user = Guid.NewGuid();
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var updated = created.AddDays(1);
        var deleted = created.AddDays(2);

        // Act
        var workGroup = new WorkGroup(WorkGroupId, "Administrator", "Description", created, user, updated, user, deleted, user, true, true, true);

        // Assert
        workGroup.CreatedBy.ShouldBe(user);
        workGroup.LastUpdatedDate.ShouldBe(updated);
        workGroup.LastUpdatedBy.ShouldBe(user);
        workGroup.DeletedDate.ShouldBe(deleted);
        workGroup.DeletedBy.ShouldBe(user);
        workGroup.IsDeleted.ShouldBeTrue();
        workGroup.IsSystem.ShouldBeTrue();
        workGroup.IsGdpr.ShouldBeTrue();
    }

    [Fact]
    public void ConstructorIsAnAuditableAndNotACompany()
    {
        // Arrange & Act
        var workGroup = WorkGroupTestData.Create(WorkGroupId, "Administrator");

        // Assert
        workGroup.ShouldBeAssignableTo<Auditable>();
        workGroup.ShouldNotBeAssignableTo<Company>();
    }

    [Fact]
    public void DifferentConstructorValuesProduceIndependentWorkGroups()
    {
        // Arrange & Act
        var first = WorkGroupTestData.Create(WorkGroupId, "First", "One");
        var second = WorkGroupTestData.Create(Guid.NewGuid(), "Second", "Two");

        // Assert
        second.Id.ShouldNotBe(first.Id);
        second.Name.ShouldNotBe(first.Name);
        second.Description.ShouldNotBe(first.Description);
    }
}
