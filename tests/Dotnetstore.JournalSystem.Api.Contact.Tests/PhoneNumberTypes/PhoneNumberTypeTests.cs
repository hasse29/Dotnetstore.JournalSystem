using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumberTypes;

public sealed class PhoneNumberTypeTests
{
    private static readonly Guid PhoneNumberTypeId = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    [Fact]
    public void ConstructorSetsAllValues()
    {
        // Arrange & Act
        var phoneNumberType = PhoneNumberTypeTestData.Create(PhoneNumberTypeId, "Administrator", "Can do everything");

        // Assert
        phoneNumberType.Id.ShouldBe(PhoneNumberTypeId);
        phoneNumberType.Name.ShouldBe("Administrator");
        phoneNumberType.Description.ShouldBe("Can do everything");
        phoneNumberType.CreatedDate.ShouldBe(PhoneNumberTypeTestData.CreatedDate);
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
        var phoneNumberType = new PhoneNumberType(PhoneNumberTypeId, "Administrator", "Description", created, user, updated, user, deleted, user, true, true, true);

        // Assert
        phoneNumberType.CreatedBy.ShouldBe(user);
        phoneNumberType.LastUpdatedDate.ShouldBe(updated);
        phoneNumberType.LastUpdatedBy.ShouldBe(user);
        phoneNumberType.DeletedDate.ShouldBe(deleted);
        phoneNumberType.DeletedBy.ShouldBe(user);
        phoneNumberType.IsDeleted.ShouldBeTrue();
        phoneNumberType.IsSystem.ShouldBeTrue();
        phoneNumberType.IsGdpr.ShouldBeTrue();
    }

    [Fact]
    public void ConstructorIsAnAuditableAndNotACompany()
    {
        // Arrange & Act
        var phoneNumberType = PhoneNumberTypeTestData.Create(PhoneNumberTypeId, "Administrator");

        // Assert
        phoneNumberType.ShouldBeAssignableTo<Auditable>();
        phoneNumberType.ShouldNotBeAssignableTo<Company>();
    }

    [Fact]
    public void DifferentConstructorValuesProduceIndependentPhoneNumberTypes()
    {
        // Arrange & Act
        var first = PhoneNumberTypeTestData.Create(PhoneNumberTypeId, "First", "One");
        var second = PhoneNumberTypeTestData.Create(Guid.NewGuid(), "Second", "Two");

        // Assert
        second.Id.ShouldNotBe(first.Id);
        second.Name.ShouldNotBe(first.Name);
        second.Description.ShouldNotBe(first.Description);
    }
}
