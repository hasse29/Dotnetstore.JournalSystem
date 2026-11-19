using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.AddressTypes;

public sealed class AddressTypeBuilderTests
{
    private static EditAddressTypeRequest CreateRequest(string name = "Administrator", string description = "Can do everything")
    {
        return new EditAddressTypeRequest(name, description);
    }

    [Fact]
    public void CreateWithCreateValuesBuildsAddressTypeFromRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var addressType = AddressTypeBuilder.Create().WithCreateValues(CreateRequest(), userId).Build();

        // Assert
        addressType.Id.ShouldNotBe(Guid.Empty);
        addressType.Name.ShouldBe("Administrator");
        addressType.Description.ShouldBe("Can do everything");
        addressType.CreatedBy.ShouldBe(userId);
        addressType.CreatedDate.ShouldBeInRange(before, DateTime.UtcNow);
        addressType.LastUpdatedDate.ShouldBeNull();
        addressType.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void CreateWithCreateValuesGeneratesNewIdEachTime()
    {
        // Arrange & Act
        var first = AddressTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();
        var second = AddressTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void CreateWithCreateValuesAllowsMissingUser()
    {
        // Arrange & Act
        var addressType = AddressTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        addressType.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void CreateWithCreateValuesRejectsEmptyUserId()
    {
        // Arrange & Act
        var act = () => AddressTypeBuilder.Create().WithCreateValues(CreateRequest(), Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithCreateValuesRejectsBlankName(string name)
    {
        // Arrange & Act
        var act = () => AddressTypeBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsTooLongName()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.AddressTypeNameMaxLength + 1);

        // Act
        var act = () => AddressTypeBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesAcceptsNameAtMaxLength()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.AddressTypeNameMaxLength);

        // Act
        var addressType = AddressTypeBuilder.Create().WithCreateValues(CreateRequest(name), null).Build();

        // Assert
        addressType.Name.ShouldBe(name);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsIdentityAndCreationData()
    {
        // Arrange
        var creator = Guid.NewGuid();
        var existing = AddressTypeBuilder.Create().WithCreateValues(CreateRequest(), creator).Build();
        var editor = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var updated = AddressTypeBuilder.Update().WithUpdateValues(existing, CreateRequest("Renamed", "Changed"), editor).Build();

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
    public void UpdateWithUpdateValuesKeepsFlagsFromExistingAddressType()
    {
        // Arrange
        var existing = AddressTypeBuilder.Delete()
            .SetId(Guid.NewGuid())
            .SetName("Administrator")
            .SetCreatedDate(AddressTypeTestData.CreatedDate)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Act
        var updated = AddressTypeBuilder.Update().WithUpdateValues(existing, CreateRequest("Other"), null).Build();

        // Assert
        updated.IsSystem.ShouldBeTrue();
        updated.IsGdpr.ShouldBeTrue();
        updated.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void UpdateWithUpdateValuesRejectsInvalidName()
    {
        // Arrange
        var existing = AddressTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Act
        var act = () => AddressTypeBuilder.Update().WithUpdateValues(existing, CreateRequest(""), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetIdRejectsEmptyGuid()
    {
        // Arrange
        var builder = AddressTypeBuilder.Delete();

        // Act
        var act = () => builder.SetId(Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetCreatedDateRejectsDefaultDate()
    {
        // Arrange
        var builder = AddressTypeBuilder.Delete();

        // Act
        var act = () => builder.SetCreatedDate(default);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetDescriptionRejectsTooLongValueButAcceptsMaxLengthAndEmpty()
    {
        // Arrange
        var builder = AddressTypeBuilder.Delete();

        // Act
        var tooLong = () => builder.SetDescription(new string('a', DataSchemeConstants.AddressTypeDescriptionMaxLength + 1));
        builder.SetDescription(new string('a', DataSchemeConstants.AddressTypeDescriptionMaxLength));
        builder.SetDescription(string.Empty);

        // Assert
        Should.Throw<ArgumentException>(tooLong);
    }

    [Fact]
    public void SetDescriptionRejectsNull()
    {
        // Arrange
        var builder = AddressTypeBuilder.Delete();

        // Act
        var act = () => builder.SetDescription(null!);

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void AuditSettersRejectEmptyUserGuids()
    {
        // Arrange
        var builder = AddressTypeBuilder.Delete();

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
        var addressType = AddressTypeBuilder.Delete()
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
        addressType.Id.ShouldBe(id);
        addressType.Name.ShouldBe("Administrator");
        addressType.CreatedDate.ShouldBe(created);
        addressType.DeletedDate.ShouldBe(deleted);
        addressType.DeletedBy.ShouldBe(user);
        addressType.IsDeleted.ShouldBeTrue();
        addressType.IsSystem.ShouldBeTrue();
        addressType.IsGdpr.ShouldBeTrue();
    }
}
