using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;

public sealed class EmailAddressMappersTests
{
    [Fact]
    public void ToEmailAddressResponseMapsAllValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var emailAddress = EmailAddressTestData.Create(id, "anna@example.com");

        // Act
        var response = emailAddress.ToEmailAddressResponse();

        // Assert
        response.Id.ShouldBe(id);
        response.EmailAddressTypeId.ShouldBe(EmailAddressTestData.EmailAddressTypeId);
        response.ObjectId.ShouldBe(EmailAddressTestData.ObjectId);
        response.Address.ShouldBe("anna@example.com");
    }

    [Fact]
    public void ToEmailAddressResponseDoesNotChangeTheAddress()
    {
        // Arrange
        var emailAddress = EmailAddressTestData.Create(Guid.NewGuid(), "Mixed.Case@Example.com");

        // Act
        var response = emailAddress.ToEmailAddressResponse();

        // Assert
        response.Address.ShouldBe("Mixed.Case@Example.com");
    }
}
