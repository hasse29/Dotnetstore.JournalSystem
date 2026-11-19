using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Addresses;

public sealed class AddressTests
{
    private static readonly Guid AddressId = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    [Fact]
    public void ConstructorSetsAllValues()
    {
        // Arrange & Act
        var address = AddressTestData.Create(AddressId, "Main Street 1", "Floor 2", "12345", "Stockholm", "Stockholm County", "Sweden", co: "c/o Anna");

        // Assert
        address.Id.ShouldBe(AddressId);
        address.AddressTypeId.ShouldBe(AddressTestData.AddressTypeId);
        address.ObjectId.ShouldBe(AddressTestData.ObjectId);
        address.CO.ShouldBe("c/o Anna");
        address.Street1.ShouldBe("Main Street 1");
        address.Street2.ShouldBe("Floor 2");
        address.PostalCode.ShouldBe("12345");
        address.City.ShouldBe("Stockholm");
        address.State.ShouldBe("Stockholm County");
        address.Country.ShouldBe("Sweden");
        address.CreatedDate.ShouldBe(AddressTestData.CreatedDate);
    }

    [Fact]
    public void ConstructorWithoutStreet2LeavesItNull()
    {
        // Arrange & Act
        var address = AddressTestData.Create(AddressId);

        // Assert
        address.Street2.ShouldBeNull();
    }

    [Fact]
    public void ConstructorWithoutCOLeavesItNull()
    {
        // Arrange & Act
        var address = AddressTestData.Create(AddressId);

        // Assert
        address.CO.ShouldBeNull();
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
        var address = new Address(AddressId, Guid.NewGuid(), Guid.NewGuid(), "c/o Bo", "Street", null, "1", "City", "State", "Country", created, user, updated, user, deleted, user, true, true, true);

        // Assert
        address.CO.ShouldBe("c/o Bo");
        address.CreatedBy.ShouldBe(user);
        address.LastUpdatedDate.ShouldBe(updated);
        address.LastUpdatedBy.ShouldBe(user);
        address.DeletedDate.ShouldBe(deleted);
        address.DeletedBy.ShouldBe(user);
        address.IsDeleted.ShouldBeTrue();
        address.IsSystem.ShouldBeTrue();
        address.IsGdpr.ShouldBeTrue();
    }

    [Fact]
    public void ConstructorIsAnAuditableAndNotACompany()
    {
        // Arrange & Act
        var address = AddressTestData.Create(AddressId);

        // Assert
        address.ShouldBeAssignableTo<Auditable>();
        address.ShouldNotBeAssignableTo<Company>();
    }

    [Fact]
    public void DifferentConstructorValuesProduceIndependentAddresses()
    {
        // Arrange & Act
        var first = AddressTestData.Create(AddressId, "First Street", city: "Stockholm");
        var second = AddressTestData.Create(Guid.NewGuid(), "Second Street", city: "Gothenburg", objectId: Guid.NewGuid());

        // Assert
        second.Id.ShouldNotBe(first.Id);
        second.Street1.ShouldNotBe(first.Street1);
        second.City.ShouldNotBe(first.City);
        second.ObjectId.ShouldNotBe(first.ObjectId);
    }
}
