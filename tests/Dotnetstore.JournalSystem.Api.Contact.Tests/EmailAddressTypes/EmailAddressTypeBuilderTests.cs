using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddressTypes;

public sealed class EmailAddressTypeBuilderTests
{
    private static EditEmailAddressTypeRequest CreateRequest(string name = "Administrator", string description = "Can do everything")
    {
        return new EditEmailAddressTypeRequest(name, description);
    }

    [Fact]
    public void CreateWithCreateValuesBuildsEmailAddressTypeFromRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var emailAddressType = EmailAddressTypeBuilder.Create().WithCreateValues(CreateRequest(), userId).Build();

        // Assert
        emailAddressType.Id.ShouldNotBe(Guid.Empty);
        emailAddressType.Name.ShouldBe("Administrator");
        emailAddressType.Description.ShouldBe("Can do everything");
        emailAddressType.CreatedBy.ShouldBe(userId);
        emailAddressType.CreatedDate.ShouldBeInRange(before, DateTime.UtcNow);
        emailAddressType.LastUpdatedDate.ShouldBeNull();
        emailAddressType.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void CreateWithCreateValuesGeneratesNewIdEachTime()
    {
        // Arrange & Act
        var first = EmailAddressTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();
        var second = EmailAddressTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void CreateWithCreateValuesAllowsMissingUser()
    {
        // Arrange & Act
        var emailAddressType = EmailAddressTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        emailAddressType.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void CreateWithCreateValuesRejectsEmptyUserId()
    {
        // Arrange & Act
        var act = () => EmailAddressTypeBuilder.Create().WithCreateValues(CreateRequest(), Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithCreateValuesRejectsBlankName(string name)
    {
        // Arrange & Act
        var act = () => EmailAddressTypeBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsTooLongName()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.EmailAddressTypeNameMaxLength + 1);

        // Act
        var act = () => EmailAddressTypeBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesAcceptsNameAtMaxLength()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.EmailAddressTypeNameMaxLength);

        // Act
        var emailAddressType = EmailAddressTypeBuilder.Create().WithCreateValues(CreateRequest(name), null).Build();

        // Assert
        emailAddressType.Name.ShouldBe(name);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsIdentityAndCreationData()
    {
        // Arrange
        var creator = Guid.NewGuid();
        var existing = EmailAddressTypeBuilder.Create().WithCreateValues(CreateRequest(), creator).Build();
        var editor = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var updated = EmailAddressTypeBuilder.Update().WithUpdateValues(existing, CreateRequest("Renamed", "Changed"), editor).Build();

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
    public void UpdateWithUpdateValuesKeepsFlagsFromExistingEmailAddressType()
    {
        // Arrange
        var existing = EmailAddressTypeBuilder.Delete()
            .SetId(Guid.NewGuid())
            .SetName("Administrator")
            .SetCreatedDate(EmailAddressTypeTestData.CreatedDate)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Act
        var updated = EmailAddressTypeBuilder.Update().WithUpdateValues(existing, CreateRequest("Other"), null).Build();

        // Assert
        updated.IsSystem.ShouldBeTrue();
        updated.IsGdpr.ShouldBeTrue();
        updated.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void UpdateWithUpdateValuesRejectsInvalidName()
    {
        // Arrange
        var existing = EmailAddressTypeBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Act
        var act = () => EmailAddressTypeBuilder.Update().WithUpdateValues(existing, CreateRequest(""), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetIdRejectsEmptyGuid()
    {
        // Arrange
        var builder = EmailAddressTypeBuilder.Delete();

        // Act
        var act = () => builder.SetId(Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetCreatedDateRejectsDefaultDate()
    {
        // Arrange
        var builder = EmailAddressTypeBuilder.Delete();

        // Act
        var act = () => builder.SetCreatedDate(default);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetDescriptionRejectsTooLongValueButAcceptsMaxLengthAndEmpty()
    {
        // Arrange
        var builder = EmailAddressTypeBuilder.Delete();

        // Act
        var tooLong = () => builder.SetDescription(new string('a', DataSchemeConstants.EmailAddressTypeDescriptionMaxLength + 1));
        builder.SetDescription(new string('a', DataSchemeConstants.EmailAddressTypeDescriptionMaxLength));
        builder.SetDescription(string.Empty);

        // Assert
        Should.Throw<ArgumentException>(tooLong);
    }

    [Fact]
    public void SetDescriptionRejectsNull()
    {
        // Arrange
        var builder = EmailAddressTypeBuilder.Delete();

        // Act
        var act = () => builder.SetDescription(null!);

        // Assert
        Should.Throw<ArgumentNullException>(act);
    }

    [Fact]
    public void AuditSettersRejectEmptyUserGuids()
    {
        // Arrange
        var builder = EmailAddressTypeBuilder.Delete();

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
        var emailAddressType = EmailAddressTypeBuilder.Delete()
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
        emailAddressType.Id.ShouldBe(id);
        emailAddressType.Name.ShouldBe("Administrator");
        emailAddressType.CreatedDate.ShouldBe(created);
        emailAddressType.DeletedDate.ShouldBe(deleted);
        emailAddressType.DeletedBy.ShouldBe(user);
        emailAddressType.IsDeleted.ShouldBeTrue();
        emailAddressType.IsSystem.ShouldBeTrue();
        emailAddressType.IsGdpr.ShouldBeTrue();
    }
}
