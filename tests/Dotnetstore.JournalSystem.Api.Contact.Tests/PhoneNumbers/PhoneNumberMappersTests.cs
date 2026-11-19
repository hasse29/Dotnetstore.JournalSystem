using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;

public sealed class PhoneNumberMappersTests
{
    [Fact]
    public void ToPhoneNumberResponseMapsAllValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var phoneNumber = PhoneNumberTestData.Create(id, "+46 70 123 45 67");

        // Act
        var response = phoneNumber.ToPhoneNumberResponse();

        // Assert
        response.Id.ShouldBe(id);
        response.PhoneNumberTypeId.ShouldBe(PhoneNumberTestData.PhoneNumberTypeId);
        response.ObjectId.ShouldBe(PhoneNumberTestData.ObjectId);
        response.Number.ShouldBe("+46 70 123 45 67");
    }

    [Fact]
    public void ToPhoneNumberResponseDoesNotChangeTheAddress()
    {
        // Arrange
        var phoneNumber = PhoneNumberTestData.Create(Guid.NewGuid(), "(08) 123 456");

        // Act
        var response = phoneNumber.ToPhoneNumberResponse();

        // Assert
        response.Number.ShouldBe("(08) 123 456");
    }
}
