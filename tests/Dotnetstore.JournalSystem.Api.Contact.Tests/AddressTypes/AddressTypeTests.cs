using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.AddressTypes;

public sealed class AddressTypeTests
{
    private static readonly Guid AddressTypeId = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    [Fact]
    public void ConstructorSetsAllValues()
    {
        // Arrange & Act
        var addressType = AddressTypeTestData.Create(AddressTypeId, "Administrator", "Can do everything");

        // Assert
        addressType.Id.ShouldBe(AddressTypeId);
        addressType.Name.ShouldBe("Administrator");
        addressType.Description.ShouldBe("Can do everything");
        addressType.CreatedDate.ShouldBe(AddressTypeTestData.CreatedDate);
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
        var addressType = new AddressType(AddressTypeId, "Administrator", "Description", created, user, updated, user, deleted, user, true, true, true);

        // Assert
        addressType.CreatedBy.ShouldBe(user);
        addressType.LastUpdatedDate.ShouldBe(updated);
        addressType.LastUpdatedBy.ShouldBe(user);
        addressType.DeletedDate.ShouldBe(deleted);
        addressType.DeletedBy.ShouldBe(user);
        addressType.IsDeleted.ShouldBeTrue();
        addressType.IsSystem.ShouldBeTrue();
        addressType.IsGdpr.ShouldBeTrue();
    }

    [Fact]
    public void ConstructorIsAnAuditableAndNotACompany()
    {
        // Arrange & Act
        var addressType = AddressTypeTestData.Create(AddressTypeId, "Administrator");

        // Assert
        addressType.ShouldBeAssignableTo<Auditable>();
        addressType.ShouldNotBeAssignableTo<Company>();
    }

    [Fact]
    public void DifferentConstructorValuesProduceIndependentAddressTypes()
    {
        // Arrange & Act
        var first = AddressTypeTestData.Create(AddressTypeId, "First", "One");
        var second = AddressTypeTestData.Create(Guid.NewGuid(), "Second", "Two");

        // Assert
        second.Id.ShouldNotBe(first.Id);
        second.Name.ShouldNotBe(first.Name);
        second.Description.ShouldNotBe(first.Description);
    }
}
