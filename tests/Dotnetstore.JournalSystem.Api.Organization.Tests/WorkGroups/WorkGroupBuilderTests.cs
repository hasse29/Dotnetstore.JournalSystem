using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.WorkGroups;

public sealed class WorkGroupBuilderTests
{
    private static EditWorkGroupRequest CreateRequest(string name = "Administrator", string description = "Can do everything")
    {
        return new EditWorkGroupRequest(name, description);
    }

    [Fact]
    public void CreateWithCreateValuesBuildsWorkGroupFromRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var workGroup = WorkGroupBuilder.Create().WithCreateValues(CreateRequest(), userId).Build();

        // Assert
        workGroup.Id.ShouldNotBe(Guid.Empty);
        workGroup.Name.ShouldBe("Administrator");
        workGroup.Description.ShouldBe("Can do everything");
        workGroup.CreatedBy.ShouldBe(userId);
        workGroup.CreatedDate.ShouldBeInRange(before, DateTime.UtcNow);
        workGroup.LastUpdatedDate.ShouldBeNull();
        workGroup.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void CreateWithCreateValuesGeneratesNewIdEachTime()
    {
        // Arrange & Act
        var first = WorkGroupBuilder.Create().WithCreateValues(CreateRequest(), null).Build();
        var second = WorkGroupBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void CreateWithCreateValuesAllowsMissingUser()
    {
        // Arrange & Act
        var workGroup = WorkGroupBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        workGroup.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void CreateWithCreateValuesRejectsEmptyUserId()
    {
        // Arrange & Act
        var act = () => WorkGroupBuilder.Create().WithCreateValues(CreateRequest(), Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithCreateValuesRejectsBlankName(string name)
    {
        // Arrange & Act
        var act = () => WorkGroupBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsTooLongName()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.WorkGroupNameMaxLength + 1);

        // Act
        var act = () => WorkGroupBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesAcceptsNameAtMaxLength()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.WorkGroupNameMaxLength);

        // Act
        var workGroup = WorkGroupBuilder.Create().WithCreateValues(CreateRequest(name), null).Build();

        // Assert
        workGroup.Name.ShouldBe(name);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsIdentityAndCreationData()
    {
        // Arrange
        var creator = Guid.NewGuid();
        var existing = WorkGroupBuilder.Create().WithCreateValues(CreateRequest(), creator).Build();
        var editor = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var updated = WorkGroupBuilder.Update().WithUpdateValues(existing, CreateRequest("Renamed", "Changed"), editor).Build();

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
    public void UpdateWithUpdateValuesKeepsFlagsFromExistingWorkGroup()
    {
        // Arrange
        var existing = WorkGroupBuilder.Delete()
            .SetId(Guid.NewGuid())
            .SetName("Administrator")
            .SetCreatedDate(WorkGroupTestData.CreatedDate)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Act
        var updated = WorkGroupBuilder.Update().WithUpdateValues(existing, CreateRequest("Other"), null).Build();

        // Assert
        updated.IsSystem.ShouldBeTrue();
        updated.IsGdpr.ShouldBeTrue();
        updated.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void UpdateWithUpdateValuesRejectsInvalidName()
    {
        // Arrange
        var existing = WorkGroupBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Act
        var act = () => WorkGroupBuilder.Update().WithUpdateValues(existing, CreateRequest(""), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetIdRejectsEmptyGuid()
    {
        // Arrange
        var builder = WorkGroupBuilder.Delete();

        // Act
        var act = () => builder.SetId(Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetCreatedDateRejectsDefaultDate()
    {
        // Arrange
        var builder = WorkGroupBuilder.Delete();

        // Act
        var act = () => builder.SetCreatedDate(default);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetDescriptionRejectsTooLongValueButAcceptsMaxLengthAndEmpty()
    {
        // Arrange
        var builder = WorkGroupBuilder.Delete();

        // Act
        var tooLong = () => builder.SetDescription(new string('a', DataSchemeConstants.WorkGroupDescriptionMaxLength + 1));
        builder.SetDescription(new string('a', DataSchemeConstants.WorkGroupDescriptionMaxLength));
        builder.SetDescription(string.Empty);

        // Assert
        Should.Throw<ArgumentException>(tooLong);
    }

    [Fact]
    public void SetDescriptionRejectsNull()
    {
        // Arrange
        var builder = WorkGroupBuilder.Delete();

        // Act
        var act = () => builder.SetDescription(null!);

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void AuditSettersRejectEmptyUserGuids()
    {
        // Arrange
        var builder = WorkGroupBuilder.Delete();

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
        var workGroup = WorkGroupBuilder.Delete()
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
        workGroup.Id.ShouldBe(id);
        workGroup.Name.ShouldBe("Administrator");
        workGroup.CreatedDate.ShouldBe(created);
        workGroup.DeletedDate.ShouldBe(deleted);
        workGroup.DeletedBy.ShouldBe(user);
        workGroup.IsDeleted.ShouldBeTrue();
        workGroup.IsSystem.ShouldBeTrue();
        workGroup.IsGdpr.ShouldBeTrue();
    }
}
