using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;

public sealed class RoleBuilderTests
{
    private static EditRoleRequest CreateRequest(string name = "Administrator", string description = "Can do everything")
    {
        return new EditRoleRequest(name, description);
    }

    [Fact]
    public void CreateWithCreateValuesBuildsRoleFromRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var role = RoleBuilder.Create().WithCreateValues(CreateRequest(), userId).Build();

        // Assert
        role.Id.ShouldNotBe(Guid.Empty);
        role.Name.ShouldBe("Administrator");
        role.Description.ShouldBe("Can do everything");
        role.CreatedBy.ShouldBe(userId);
        role.CreatedDate.ShouldBeInRange(before, DateTime.UtcNow);
        role.LastUpdatedDate.ShouldBeNull();
        role.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void CreateWithCreateValuesGeneratesNewIdEachTime()
    {
        // Arrange & Act
        var first = RoleBuilder.Create().WithCreateValues(CreateRequest(), null).Build();
        var second = RoleBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void CreateWithCreateValuesAllowsMissingUser()
    {
        // Arrange & Act
        var role = RoleBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        role.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void CreateWithCreateValuesRejectsEmptyUserId()
    {
        // Arrange & Act
        var act = () => RoleBuilder.Create().WithCreateValues(CreateRequest(), Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithCreateValuesRejectsBlankName(string name)
    {
        // Arrange & Act
        var act = () => RoleBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsTooLongName()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.RoleNameMaxLength + 1);

        // Act
        var act = () => RoleBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesAcceptsNameAtMaxLength()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.RoleNameMaxLength);

        // Act
        var role = RoleBuilder.Create().WithCreateValues(CreateRequest(name), null).Build();

        // Assert
        role.Name.ShouldBe(name);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsIdentityAndCreationData()
    {
        // Arrange
        var creator = Guid.NewGuid();
        var existing = RoleBuilder.Create().WithCreateValues(CreateRequest(), creator).Build();
        var editor = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var updated = RoleBuilder.Update().WithUpdateValues(existing, CreateRequest("Renamed", "Changed"), editor).Build();

        // Assert
        updated.Id.ShouldBe(existing.Id);
        updated.CreatedDate.ShouldBe(existing.CreatedDate);
        updated.CreatedBy.ShouldBe(creator);
        updated.Name.ShouldBe("Renamed");
        updated.Description.ShouldBe("Changed");
        updated.LastUpdatedBy.ShouldBe(editor);
        updated.LastUpdatedDate.ShouldNotBeNull().ShouldBeInRange(before, DateTime.UtcNow);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsFlagsFromExistingRole()
    {
        // Arrange
        var existing = RoleBuilder.Delete()
            .SetId(Guid.NewGuid())
            .SetName("Administrator")
            .SetCreatedDate(RoleTestData.CreatedDate)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Act
        var updated = RoleBuilder.Update().WithUpdateValues(existing, CreateRequest("Other"), null).Build();

        // Assert
        updated.IsSystem.ShouldBeTrue();
        updated.IsGdpr.ShouldBeTrue();
        updated.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void UpdateWithUpdateValuesRejectsInvalidName()
    {
        // Arrange
        var existing = RoleBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Act
        var act = () => RoleBuilder.Update().WithUpdateValues(existing, CreateRequest(""), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetIdRejectsEmptyGuid()
    {
        // Arrange
        var builder = RoleBuilder.Delete();

        // Act
        var act = () => builder.SetId(Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetCreatedDateRejectsDefaultDate()
    {
        // Arrange
        var builder = RoleBuilder.Delete();

        // Act
        var act = () => builder.SetCreatedDate(default);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetDescriptionRejectsTooLongValueButAcceptsMaxLengthAndEmpty()
    {
        // Arrange
        var builder = RoleBuilder.Delete();

        // Act
        var tooLong = () => builder.SetDescription(new string('a', DataSchemeConstants.RoleDescriptionMaxLength + 1));
        builder.SetDescription(new string('a', DataSchemeConstants.RoleDescriptionMaxLength));
        builder.SetDescription(string.Empty);

        // Assert
        Should.Throw<ArgumentException>(tooLong);
    }

    [Fact]
    public void SetDescriptionRejectsNull()
    {
        // Arrange
        var builder = RoleBuilder.Delete();

        // Act
        var act = () => builder.SetDescription(null!);

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void AuditSettersRejectEmptyUserGuids()
    {
        // Arrange
        var builder = RoleBuilder.Delete();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.SetCreatedBy(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetLastUpdatedBy(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetDeletedBy(Guid.Empty));
    }

    [Fact]
    public void BuildCarriesAllSetValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = Guid.NewGuid();
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var deleted = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var role = RoleBuilder.Delete()
            .SetId(id)
            .SetName("Administrator")
            .SetCreatedDate(created)
            .SetCreatedBy(user)
            .SetDeletedDate(deleted)
            .SetDeletedBy(user)
            .SetIsDeleted(true)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Assert
        role.Id.ShouldBe(id);
        role.Name.ShouldBe("Administrator");
        role.CreatedDate.ShouldBe(created);
        role.DeletedDate.ShouldBe(deleted);
        role.DeletedBy.ShouldBe(user);
        role.IsDeleted.ShouldBeTrue();
        role.IsSystem.ShouldBeTrue();
        role.IsGdpr.ShouldBeTrue();
    }
}
