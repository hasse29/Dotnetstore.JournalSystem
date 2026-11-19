using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;

public sealed class PhoneNumberBuilderTests
{
    private static EditPhoneNumberRequest CreateRequest(string number = "+46 70 123 45 67")
    {
        return new EditPhoneNumberRequest(PhoneNumberTestData.PhoneNumberTypeId, PhoneNumberTestData.ObjectId, number);
    }

    [Fact]
    public void CreateWithCreateValuesBuildsPhoneNumberFromRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var phoneNumber = PhoneNumberBuilder.Create().WithCreateValues(CreateRequest(), userId).Build();

        // Assert
        phoneNumber.Id.ShouldNotBe(Guid.Empty);
        phoneNumber.PhoneNumberTypeId.ShouldBe(PhoneNumberTestData.PhoneNumberTypeId);
        phoneNumber.ObjectId.ShouldBe(PhoneNumberTestData.ObjectId);
        phoneNumber.Number.ShouldBe("+46 70 123 45 67");
        phoneNumber.CreatedBy.ShouldBe(userId);
        phoneNumber.CreatedDate.ShouldBeInRange(before, DateTime.UtcNow);
        phoneNumber.LastUpdatedDate.ShouldBeNull();
        phoneNumber.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void CreateWithCreateValuesGeneratesNewIdEachTime()
    {
        // Arrange & Act
        var first = PhoneNumberBuilder.Create().WithCreateValues(CreateRequest(), null).Build();
        var second = PhoneNumberBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void CreateWithCreateValuesAllowsMissingUser()
    {
        // Arrange & Act
        var phoneNumber = PhoneNumberBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        phoneNumber.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void CreateWithCreateValuesRejectsEmptyUserId()
    {
        // Arrange & Act
        var act = () => PhoneNumberBuilder.Create().WithCreateValues(CreateRequest(), Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-number")]
    [InlineData("Call me 0701234567")]
    public void CreateWithCreateValuesRejectsBlankOrInvalidAddress(string number)
    {
        // Arrange & Act
        var act = () => PhoneNumberBuilder.Create().WithCreateValues(CreateRequest(number), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsTooLongNumber()
    {
        // Arrange
        var number = new string('1', DataSchemeConstants.PhoneNumberMaxLength + 1);

        // Act
        var act = () => PhoneNumberBuilder.Create().WithCreateValues(CreateRequest(number), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsIdentityAndCreationData()
    {
        // Arrange
        var creator = Guid.NewGuid();
        var existing = PhoneNumberBuilder.Create().WithCreateValues(CreateRequest(), creator).Build();
        var editor = Guid.NewGuid();
        var newOwner = Guid.NewGuid();
        var request = new EditPhoneNumberRequest(PhoneNumberTestData.PhoneNumberTypeId, newOwner, "+46 70 333 33 33");
        var before = DateTime.UtcNow;

        // Act
        var updated = PhoneNumberBuilder.Update().WithUpdateValues(existing, request, editor).Build();

        // Assert
        updated.Id.ShouldBe(existing.Id);
        updated.CreatedDate.ShouldBe(existing.CreatedDate);
        updated.CreatedBy.ShouldBe(creator);
        updated.ObjectId.ShouldBe(newOwner);
        updated.Number.ShouldBe("+46 70 333 33 33");
        updated.LastUpdatedBy.ShouldBe(editor);
        updated.LastUpdatedDate.ShouldNotBeNull().ShouldBeInRange(before, DateTime.UtcNow);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsFlagsFromExistingPhoneNumber()
    {
        // Arrange
        var existing = PhoneNumberBuilder.Delete()
            .SetId(Guid.NewGuid())
            .SetPhoneNumberTypeId(PhoneNumberTestData.PhoneNumberTypeId)
            .SetObjectId(PhoneNumberTestData.ObjectId)
            .SetNumber("+46 70 123 45 67")
            .SetCreatedDate(PhoneNumberTestData.CreatedDate)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Act
        var updated = PhoneNumberBuilder.Update().WithUpdateValues(existing, CreateRequest("+46 70 666 66 66"), null).Build();

        // Assert
        updated.IsSystem.ShouldBeTrue();
        updated.IsGdpr.ShouldBeTrue();
        updated.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void UpdateWithUpdateValuesRejectsInvalidAddress()
    {
        // Arrange
        var existing = PhoneNumberBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Act
        var act = () => PhoneNumberBuilder.Update().WithUpdateValues(existing, CreateRequest("nope"), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetIdAndReferenceIdsRejectEmptyGuid()
    {
        // Arrange
        var builder = PhoneNumberBuilder.Delete();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.SetId(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetPhoneNumberTypeId(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetObjectId(Guid.Empty));
    }

    [Fact]
    public void SetCreatedDateRejectsDefaultDate()
    {
        // Arrange
        var builder = PhoneNumberBuilder.Delete();

        // Act
        var act = () => builder.SetCreatedDate(default);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void AuditSettersRejectEmptyUserGuids()
    {
        // Arrange
        var builder = PhoneNumberBuilder.Delete();

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
        var phoneNumber = PhoneNumberBuilder.Delete()
            .SetId(id)
            .SetNumber("+46 70 123 45 67")
            .SetCreatedDate(created)
            .SetCreatedBy(user)
            .SetDeletedDate(deleted)
            .SetDeletedBy(user)
            .SetIsDeleted(true)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Assert
        phoneNumber.Id.ShouldBe(id);
        phoneNumber.Number.ShouldBe("+46 70 123 45 67");
        phoneNumber.CreatedDate.ShouldBe(created);
        phoneNumber.DeletedDate.ShouldBe(deleted);
        phoneNumber.DeletedBy.ShouldBe(user);
        phoneNumber.IsDeleted.ShouldBeTrue();
        phoneNumber.IsSystem.ShouldBeTrue();
        phoneNumber.IsGdpr.ShouldBeTrue();
    }
}
