using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;

public sealed class RoleTests
{
    private static readonly Guid RoleId = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    [Fact]
    public void ConstructorSetsAllValues()
    {
        // Arrange & Act
        var role = RoleTestData.Create(RoleId, "Administrator", "Can do everything");

        // Assert
        role.Id.ShouldBe(RoleId);
        role.Name.ShouldBe("Administrator");
        role.Description.ShouldBe("Can do everything");
        role.CreatedDate.ShouldBe(RoleTestData.CreatedDate);
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
        var role = new Role(RoleId, "Administrator", "Description", created, user, updated, user, deleted, user, true, true, true);

        // Assert
        role.CreatedBy.ShouldBe(user);
        role.LastUpdatedDate.ShouldBe(updated);
        role.LastUpdatedBy.ShouldBe(user);
        role.DeletedDate.ShouldBe(deleted);
        role.DeletedBy.ShouldBe(user);
        role.IsDeleted.ShouldBeTrue();
        role.IsSystem.ShouldBeTrue();
        role.IsGdpr.ShouldBeTrue();
    }

    [Fact]
    public void ConstructorIsAnAuditableAndNotACompany()
    {
        // Arrange & Act
        var role = RoleTestData.Create(RoleId, "Administrator");

        // Assert
        role.ShouldBeAssignableTo<Auditable>();
        role.ShouldNotBeAssignableTo<Company>();
    }

    [Fact]
    public void DifferentConstructorValuesProduceIndependentRoles()
    {
        // Arrange & Act
        var first = RoleTestData.Create(RoleId, "First", "One");
        var second = RoleTestData.Create(Guid.NewGuid(), "Second", "Two");

        // Assert
        second.Id.ShouldNotBe(first.Id);
        second.Name.ShouldNotBe(first.Name);
        second.Description.ShouldNotBe(first.Description);
    }
}
