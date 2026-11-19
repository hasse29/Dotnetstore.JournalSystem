using System.Security.Cryptography;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;

public sealed class PhoneNumberEncryptionTests
{
    private static readonly Guid Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void EncryptReplacesTheNumberWithCiphertext()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var phoneNumber = PhoneNumberTestData.Create(Id);

        // Act
        var encrypted = phoneNumber.Encrypt(handler);

        // Assert
        encrypted.Number.ShouldStartWith("dse1.");
        encrypted.Number.ShouldNotContain("70 123");
    }

    [Fact]
    public void EncryptKeepsNonSensitiveValuesAndDoesNotChangeTheOriginal()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var phoneNumber = PhoneNumberTestData.Create(Id);

        // Act
        var encrypted = phoneNumber.Encrypt(handler);

        // Assert
        encrypted.Id.ShouldBe(phoneNumber.Id);
        encrypted.PhoneNumberTypeId.ShouldBe(phoneNumber.PhoneNumberTypeId);
        encrypted.ObjectId.ShouldBe(phoneNumber.ObjectId);
        phoneNumber.Number.ShouldBe("+46 70 123 45 67");
    }

    [Fact]
    public void DecryptRestoresTheOriginalNumber()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var phoneNumber = PhoneNumberTestData.Create(Id);

        // Act
        var decrypted = phoneNumber.Encrypt(handler).Decrypt(handler);

        // Assert
        decrypted.Number.ShouldBe("+46 70 123 45 67");
    }

    [Fact]
    public void DecryptRejectsPlaintext()
    {
        // Arrange
        var handler = TestEncryption.Create();

        // Act
        var action = () => PhoneNumberTestData.Create(Id).Decrypt(handler);

        // Assert
        action.ShouldThrow<CryptographicException>();
    }

    [Fact]
    public void DecryptRejectsCiphertextMovedToAnotherPhoneNumber()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var encrypted = PhoneNumberTestData.Create(Id).Encrypt(handler);
        var moved = PhoneNumberTestData.Create(Guid.NewGuid(), encrypted.Number);

        // Act
        var action = () => moved.Decrypt(handler);

        // Assert
        action.ShouldThrow<CryptographicException>();
    }
}
