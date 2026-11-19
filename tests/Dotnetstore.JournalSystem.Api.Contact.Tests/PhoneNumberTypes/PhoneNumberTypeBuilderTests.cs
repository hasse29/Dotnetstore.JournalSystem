using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumberTypes;

public sealed class PhoneNumberTypeBuilderTests
{
    private static EditPhoneNumberTypeRequest CreateRequest(string name = "Administrator", string description = "Can do everything")
    {
        return new EditPhoneNumberTypeRequest(name, description);
    }

    [Fact]
    public void CreateWithCreateValuesBuildsPhoneNumberTypeFromRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var phoneNumberType = PhoneNumberTypeBuilder.Create().WithCreateValues(CreateRequest(), userId).Build();

        // Assert
        phoneNumberType.Id.ShouldNotBe(Guid.Empty);
        phoneNumberType.Name.ShouldBe("Administrator");
        phoneNumberType.Description.ShouldBe("Can do everything");
        phoneNumberType.CreatedBy.ShouldBe(userId);
        phoneNumberType.CreatedDate.ShouldBeInRange(before, DateTime.UtcNow);
        phoneNumberType.LastUpdatedDate.ShouldBeNull();
        phoneNumberType.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void CreateWithCreateValuesGeneratesNewIdEachTime()
    {
        // Arrange & Act
        var first = PhoneNumberTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();
        var second = PhoneNumberTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void CreateWithCreateValuesAllowsMissingUser()
    {
        // Arrange & Act
        var phoneNumberType = PhoneNumberTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        phoneNumberType.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void CreateWithCreateValuesRejectsEmptyUserId()
    {
        // Arrange & Act
        var act = () => PhoneNumberTypeBuilder.Create().WithCreateValues(CreateRequest(), Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithCreateValuesRejectsBlankName(string name)
    {
        // Arrange & Act
        var act = () => PhoneNumberTypeBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsTooLongName()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.PhoneNumberTypeNameMaxLength + 1);

        // Act
        var act = () => PhoneNumberTypeBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesAcceptsNameAtMaxLength()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.PhoneNumberTypeNameMaxLength);

        // Act
        var phoneNumberType = PhoneNumberTypeBuilder.Create().WithCreateValues(CreateRequest(name), null).Build();

        // Assert
        phoneNumberType.Name.ShouldBe(name);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsIdentityAndCreationData()
    {
        // Arrange
        var creator = Guid.NewGuid();
        var existing = PhoneNumberTypeBuilder.Create().WithCreateValues(CreateRequest(), creator).Build();
        var editor = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var updated = PhoneNumberTypeBuilder.Update().WithUpdateValues(existing, CreateRequest("Renamed", "Changed"), editor).Build();

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
    public void UpdateWithUpdateValuesKeepsFlagsFromExistingPhoneNumberType()
    {
        // Arrange
        var existing = PhoneNumberTypeBuilder.Delete()
            .SetId(Guid.NewGuid())
            .SetName("Administrator")
            .SetCreatedDate(PhoneNumberTypeTestData.CreatedDate)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Act
        var updated = PhoneNumberTypeBuilder.Update().WithUpdateValues(existing, CreateRequest("Other"), null).Build();

        // Assert
        updated.IsSystem.ShouldBeTrue();
        updated.IsGdpr.ShouldBeTrue();
        updated.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void UpdateWithUpdateValuesRejectsInvalidName()
    {
        // Arrange
        var existing = PhoneNumberTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Act
        var act = () => PhoneNumberTypeBuilder.Update().WithUpdateValues(existing, CreateRequest(""), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetIdRejectsEmptyGuid()
    {
        // Arrange
        var builder = PhoneNumberTypeBuilder.Delete();

        // Act
        var act = () => builder.SetId(Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetCreatedDateRejectsDefaultDate()
    {
        // Arrange
        var builder = PhoneNumberTypeBuilder.Delete();

        // Act
        var act = () => builder.SetCreatedDate(default);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetDescriptionRejectsTooLongValueButAcceptsMaxLengthAndEmpty()
    {
        // Arrange
        var builder = PhoneNumberTypeBuilder.Delete();

        // Act
        var tooLong = () => builder.SetDescription(new string('a', DataSchemeConstants.PhoneNumberTypeDescriptionMaxLength + 1));
        builder.SetDescription(new string('a', DataSchemeConstants.PhoneNumberTypeDescriptionMaxLength));
        builder.SetDescription(string.Empty);

        // Assert
        Should.Throw<ArgumentException>(tooLong);
    }

    [Fact]
    public void SetDescriptionRejectsNull()
    {
        // Arrange
        var builder = PhoneNumberTypeBuilder.Delete();

        // Act
        var act = () => builder.SetDescription(null!);

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void AuditSettersRejectEmptyUserGuids()
    {
        // Arrange
        var builder = PhoneNumberTypeBuilder.Delete();

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
        var phoneNumberType = PhoneNumberTypeBuilder.Delete()
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
        phoneNumberType.Id.ShouldBe(id);
        phoneNumberType.Name.ShouldBe("Administrator");
        phoneNumberType.CreatedDate.ShouldBe(created);
        phoneNumberType.DeletedDate.ShouldBe(deleted);
        phoneNumberType.DeletedBy.ShouldBe(user);
        phoneNumberType.IsDeleted.ShouldBeTrue();
        phoneNumberType.IsSystem.ShouldBeTrue();
        phoneNumberType.IsGdpr.ShouldBeTrue();
    }
}
