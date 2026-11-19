using System.Security.Cryptography;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;

public sealed class EmailAddressEncryptionTests
{
    private static readonly Guid Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void EncryptReplacesTheAddressWithCiphertext()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var emailAddress = EmailAddressTestData.Create(Id);

        // Act
        var encrypted = emailAddress.Encrypt(handler);

        // Assert
        encrypted.Address.ShouldStartWith("dse1.");
        encrypted.Address.ShouldNotContain("anna");
    }

    [Fact]
    public void EncryptKeepsNonSensitiveValuesAndDoesNotChangeTheOriginal()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var emailAddress = EmailAddressTestData.Create(Id);

        // Act
        var encrypted = emailAddress.Encrypt(handler);

        // Assert
        encrypted.Id.ShouldBe(emailAddress.Id);
        encrypted.EmailAddressTypeId.ShouldBe(emailAddress.EmailAddressTypeId);
        encrypted.ObjectId.ShouldBe(emailAddress.ObjectId);
        emailAddress.Address.ShouldBe("anna@example.com");
    }

    [Fact]
    public void DecryptRestoresTheOriginalAddress()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var emailAddress = EmailAddressTestData.Create(Id);

        // Act
        var decrypted = emailAddress.Encrypt(handler).Decrypt(handler);

        // Assert
        decrypted.Address.ShouldBe("anna@example.com");
    }

    [Fact]
    public void DecryptRejectsPlaintext()
    {
        // Arrange
        var handler = TestEncryption.Create();

        // Act
        var action = () => EmailAddressTestData.Create(Id).Decrypt(handler);

        // Assert
        action.ShouldThrow<CryptographicException>();
    }

    [Fact]
    public void DecryptRejectsCiphertextMovedToAnotherEmailAddress()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var encrypted = EmailAddressTestData.Create(Id).Encrypt(handler);
        var moved = EmailAddressTestData.Create(Guid.NewGuid(), encrypted.Address);

        // Act
        var action = () => moved.Decrypt(handler);

        // Assert
        action.ShouldThrow<CryptographicException>();
    }
}
