using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddressTypes;

public sealed class EmailAddressTypeMappersTests
{
    [Fact]
    public void ToEmailAddressTypeResponseMapsAllValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var emailAddressType = EmailAddressTypeTestData.Create(id, "Administrator", "Can do everything");

        // Act
        var response = emailAddressType.ToEmailAddressTypeResponse();

        // Assert
        response.Id.ShouldBe(id);
        response.Name.ShouldBe("Administrator");
        response.Description.ShouldBe("Can do everything");
    }

    [Fact]
    public void ToEmailAddressTypeResponseKeepsEmptyDescription()
    {
        // Arrange
        var emailAddressType = EmailAddressTypeTestData.Create(Guid.NewGuid(), "Administrator", string.Empty);

        // Act
        var response = emailAddressType.ToEmailAddressTypeResponse();

        // Assert
        response.Description.ShouldBeEmpty();
    }
}
