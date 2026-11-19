using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Addresses;

public sealed class AddressMappersTests
{
    [Fact]
    public void ToAddressResponseMapsAllValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var address = AddressTestData.Create(id, "Main Street 1", "Floor 2", "12345", "Stockholm", "Stockholm County", "Sweden", co: "c/o Anna");

        // Act
        var response = address.ToAddressResponse();

        // Assert
        response.Id.ShouldBe(id);
        response.AddressTypeId.ShouldBe(AddressTestData.AddressTypeId);
        response.ObjectId.ShouldBe(AddressTestData.ObjectId);
        response.CO.ShouldBe("c/o Anna");
        response.Street1.ShouldBe("Main Street 1");
        response.Street2.ShouldBe("Floor 2");
        response.PostalCode.ShouldBe("12345");
        response.City.ShouldBe("Stockholm");
        response.State.ShouldBe("Stockholm County");
        response.Country.ShouldBe("Sweden");
    }

    [Fact]
    public void ToAddressResponseKeepsStreet2AndCONull()
    {
        // Arrange
        var address = AddressTestData.Create(Guid.NewGuid());

        // Act
        var response = address.ToAddressResponse();

        // Assert
        response.CO.ShouldBeNull();
        response.Street2.ShouldBeNull();
    }
}
