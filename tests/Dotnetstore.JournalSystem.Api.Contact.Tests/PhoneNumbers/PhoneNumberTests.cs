using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;

public sealed class PhoneNumberTests
{
    private static readonly Guid PhoneNumberId = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    [Fact]
    public void ConstructorSetsAllValues()
    {
        // Arrange & Act
        var phoneNumber = PhoneNumberTestData.Create(PhoneNumberId, "+46 70 123 45 67");

        // Assert
        phoneNumber.Id.ShouldBe(PhoneNumberId);
        phoneNumber.PhoneNumberTypeId.ShouldBe(PhoneNumberTestData.PhoneNumberTypeId);
        phoneNumber.ObjectId.ShouldBe(PhoneNumberTestData.ObjectId);
        phoneNumber.Number.ShouldBe("+46 70 123 45 67");
        phoneNumber.CreatedDate.ShouldBe(PhoneNumberTestData.CreatedDate);
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
        var phoneNumber = new PhoneNumber(PhoneNumberId, Guid.NewGuid(), Guid.NewGuid(), "12345", created, user, updated, user, deleted, user, true, true, true);

        // Assert
        phoneNumber.CreatedBy.ShouldBe(user);
        phoneNumber.LastUpdatedDate.ShouldBe(updated);
        phoneNumber.LastUpdatedBy.ShouldBe(user);
        phoneNumber.DeletedDate.ShouldBe(deleted);
        phoneNumber.DeletedBy.ShouldBe(user);
        phoneNumber.IsDeleted.ShouldBeTrue();
        phoneNumber.IsSystem.ShouldBeTrue();
        phoneNumber.IsGdpr.ShouldBeTrue();
    }

    [Fact]
    public void ConstructorIsAnAuditableAndNotACompany()
    {
        // Arrange & Act
        var phoneNumber = PhoneNumberTestData.Create(PhoneNumberId);

        // Assert
        phoneNumber.ShouldBeAssignableTo<Auditable>();
        phoneNumber.ShouldNotBeAssignableTo<Company>();
    }

    [Fact]
    public void DifferentConstructorValuesProduceIndependentPhoneNumbers()
    {
        // Arrange & Act
        var first = PhoneNumberTestData.Create(PhoneNumberId, "+46 70 111 11 11");
        var second = PhoneNumberTestData.Create(Guid.NewGuid(), "+46 70 222 22 22", objectId: Guid.NewGuid());

        // Assert
        second.Id.ShouldNotBe(first.Id);
        second.Number.ShouldNotBe(first.Number);
        second.ObjectId.ShouldNotBe(first.ObjectId);
    }
}
