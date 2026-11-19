using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.AddressTypes;

public sealed class AddressTypeMappersTests
{
    [Fact]
    public void ToAddressTypeResponseMapsAllValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var addressType = AddressTypeTestData.Create(id, "Administrator", "Can do everything");

        // Act
        var response = addressType.ToAddressTypeResponse();

        // Assert
        response.Id.ShouldBe(id);
        response.Name.ShouldBe("Administrator");
        response.Description.ShouldBe("Can do everything");
    }

    [Fact]
    public void ToAddressTypeResponseKeepsEmptyDescription()
    {
        // Arrange
        var addressType = AddressTypeTestData.Create(Guid.NewGuid(), "Administrator", string.Empty);

        // Act
        var response = addressType.ToAddressTypeResponse();

        // Assert
        response.Description.ShouldBeEmpty();
    }
}
