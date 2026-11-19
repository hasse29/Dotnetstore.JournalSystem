using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumberTypes;

public sealed class PhoneNumberTypeMappersTests
{
    [Fact]
    public void ToPhoneNumberTypeResponseMapsAllValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var phoneNumberType = PhoneNumberTypeTestData.Create(id, "Administrator", "Can do everything");

        // Act
        var response = phoneNumberType.ToPhoneNumberTypeResponse();

        // Assert
        response.Id.ShouldBe(id);
        response.Name.ShouldBe("Administrator");
        response.Description.ShouldBe("Can do everything");
    }

    [Fact]
    public void ToPhoneNumberTypeResponseKeepsEmptyDescription()
    {
        // Arrange
        var phoneNumberType = PhoneNumberTypeTestData.Create(Guid.NewGuid(), "Administrator", string.Empty);

        // Act
        var response = phoneNumberType.ToPhoneNumberTypeResponse();

        // Assert
        response.Description.ShouldBeEmpty();
    }
}
