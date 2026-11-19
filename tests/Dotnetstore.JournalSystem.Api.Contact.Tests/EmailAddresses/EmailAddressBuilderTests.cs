using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;

public sealed class EmailAddressBuilderTests
{
    private static EditEmailAddressRequest CreateRequest(string address = "anna@example.com")
    {
        return new EditEmailAddressRequest(EmailAddressTestData.EmailAddressTypeId, EmailAddressTestData.ObjectId, address);
    }

    [Fact]
    public void CreateWithCreateValuesBuildsEmailAddressFromRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var emailAddress = EmailAddressBuilder.Create().WithCreateValues(CreateRequest(), userId).Build();

        // Assert
        emailAddress.Id.ShouldNotBe(Guid.Empty);
        emailAddress.EmailAddressTypeId.ShouldBe(EmailAddressTestData.EmailAddressTypeId);
        emailAddress.ObjectId.ShouldBe(EmailAddressTestData.ObjectId);
        emailAddress.Address.ShouldBe("anna@example.com");
        emailAddress.CreatedBy.ShouldBe(userId);
        emailAddress.CreatedDate.ShouldBeInRange(before, DateTime.UtcNow);
        emailAddress.LastUpdatedDate.ShouldBeNull();
        emailAddress.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void CreateWithCreateValuesGeneratesNewIdEachTime()
    {
        // Arrange & Act
        var first = EmailAddressBuilder.Create().WithCreateValues(CreateRequest(), null).Build();
        var second = EmailAddressBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void CreateWithCreateValuesAllowsMissingUser()
    {
        // Arrange & Act
        var emailAddress = EmailAddressBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        emailAddress.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void CreateWithCreateValuesRejectsEmptyUserId()
    {
        // Arrange & Act
        var act = () => EmailAddressBuilder.Create().WithCreateValues(CreateRequest(), Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("Anna <anna@example.com>")]
    public void CreateWithCreateValuesRejectsBlankOrInvalidAddress(string address)
    {
        // Arrange & Act
        var act = () => EmailAddressBuilder.Create().WithCreateValues(CreateRequest(address), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsTooLongAddress()
    {
        // Arrange
        var address = new string('a', DataSchemeConstants.EmailAddressMaxLength) + "@example.com";

        // Act
        var act = () => EmailAddressBuilder.Create().WithCreateValues(CreateRequest(address), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsIdentityAndCreationData()
    {
        // Arrange
        var creator = Guid.NewGuid();
        var existing = EmailAddressBuilder.Create().WithCreateValues(CreateRequest(), creator).Build();
        var editor = Guid.NewGuid();
        var newOwner = Guid.NewGuid();
        var request = new EditEmailAddressRequest(EmailAddressTestData.EmailAddressTypeId, newOwner, "new@example.com");
        var before = DateTime.UtcNow;

        // Act
        var updated = EmailAddressBuilder.Update().WithUpdateValues(existing, request, editor).Build();

        // Assert
        updated.Id.ShouldBe(existing.Id);
        updated.CreatedDate.ShouldBe(existing.CreatedDate);
        updated.CreatedBy.ShouldBe(creator);
        updated.ObjectId.ShouldBe(newOwner);
        updated.Address.ShouldBe("new@example.com");
        updated.LastUpdatedBy.ShouldBe(editor);
        updated.LastUpdatedDate.ShouldNotBeNull().ShouldBeInRange(before, DateTime.UtcNow);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsFlagsFromExistingEmailAddress()
    {
        // Arrange
        var existing = EmailAddressBuilder.Delete()
            .SetId(Guid.NewGuid())
            .SetEmailAddressTypeId(EmailAddressTestData.EmailAddressTypeId)
            .SetObjectId(EmailAddressTestData.ObjectId)
            .SetAddress("anna@example.com")
            .SetCreatedDate(EmailAddressTestData.CreatedDate)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Act
        var updated = EmailAddressBuilder.Update().WithUpdateValues(existing, CreateRequest("other@example.com"), null).Build();

        // Assert
        updated.IsSystem.ShouldBeTrue();
        updated.IsGdpr.ShouldBeTrue();
        updated.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void UpdateWithUpdateValuesRejectsInvalidAddress()
    {
        // Arrange
        var existing = EmailAddressBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Act
        var act = () => EmailAddressBuilder.Update().WithUpdateValues(existing, CreateRequest("nope"), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetIdAndReferenceIdsRejectEmptyGuid()
    {
        // Arrange
        var builder = EmailAddressBuilder.Delete();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.SetId(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetEmailAddressTypeId(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetObjectId(Guid.Empty));
    }

    [Fact]
    public void SetCreatedDateRejectsDefaultDate()
    {
        // Arrange
        var builder = EmailAddressBuilder.Delete();

        // Act
        var act = () => builder.SetCreatedDate(default);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void AuditSettersRejectEmptyUserGuids()
    {
        // Arrange
        var builder = EmailAddressBuilder.Delete();

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
        var emailAddress = EmailAddressBuilder.Delete()
            .SetId(id)
            .SetAddress("anna@example.com")
            .SetCreatedDate(created)
            .SetCreatedBy(user)
            .SetDeletedDate(deleted)
            .SetDeletedBy(user)
            .SetIsDeleted(true)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Assert
        emailAddress.Id.ShouldBe(id);
        emailAddress.Address.ShouldBe("anna@example.com");
        emailAddress.CreatedDate.ShouldBe(created);
        emailAddress.DeletedDate.ShouldBe(deleted);
        emailAddress.DeletedBy.ShouldBe(user);
        emailAddress.IsDeleted.ShouldBeTrue();
        emailAddress.IsSystem.ShouldBeTrue();
        emailAddress.IsGdpr.ShouldBeTrue();
    }
}
