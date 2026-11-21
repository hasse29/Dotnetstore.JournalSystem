using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class PersonalIdentityNumberProtectorTests
{
    private readonly IPersonalIdentityNumberProtector _protector = new PersonalIdentityNumberProtector(TestEncryption.Create());

    [Fact]
    public void ProtectEncryptsTheNumberSoItIsNotStoredInThePlain()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var stored = _protector.Protect("19900101-1234", userId);

        // Assert
        stored.ShouldNotBeNullOrWhiteSpace();
        stored.ShouldNotContain("19900101");
    }

    [Fact]
    public void ProtectReturnsNullForBlankInput()
    {
        // Arrange & Act
        var stored = _protector.Protect("  ", Guid.NewGuid());

        // Assert
        stored.ShouldBeNull();
    }

    [Fact]
    public void MaskRoundTripsToTheMaskedFormOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var stored = _protector.Protect("19900101-1234", userId);

        // Act
        var masked = _protector.Mask(stored, userId);

        // Assert
        masked.ShouldBe("19900101-****");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void MaskReturnsNullWhenNothingIsStored(string? stored)
    {
        // Arrange & Act
        var masked = _protector.Mask(stored, Guid.NewGuid());

        // Assert
        masked.ShouldBeNull();
    }

    [Fact]
    public void AValueCopiedToAnotherUserCannotBeRead()
    {
        // Arrange
        var stored = _protector.Protect("19900101-1234", Guid.NewGuid());

        // Act
        var read = () => _protector.Mask(stored, Guid.NewGuid());

        // Assert
        Should.Throw<Exception>(read);
    }
}
