using System.Security.Cryptography;
using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Addresses;

public sealed class AddressEncryptionTests
{
    private static readonly Guid Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void EncryptReplacesEveryTextFieldWithCiphertext()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var address = AddressTestData.Create(Id, street2: "Floor 2", co: "c/o Anna");

        // Act
        var encrypted = address.Encrypt(handler);

        // Assert
        new[] { encrypted.CO, encrypted.Street1, encrypted.Street2, encrypted.PostalCode, encrypted.City, encrypted.State, encrypted.Country }
            .ShouldAllBe(x => x != null && x.StartsWith("dse1.", StringComparison.Ordinal));
    }

    [Fact]
    public void EncryptKeepsNonSensitiveValues()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var address = AddressTestData.Create(Id);

        // Act
        var encrypted = address.Encrypt(handler);

        // Assert
        encrypted.Id.ShouldBe(address.Id);
        encrypted.AddressTypeId.ShouldBe(address.AddressTypeId);
        encrypted.ObjectId.ShouldBe(address.ObjectId);
        encrypted.CreatedDate.ShouldBe(address.CreatedDate);
    }

    [Fact]
    public void EncryptLeavesOptionalFieldsNullWhenTheyAreNull()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var address = AddressTestData.Create(Id, street2: null, co: null);

        // Act
        var encrypted = address.Encrypt(handler);

        // Assert
        encrypted.CO.ShouldBeNull();
        encrypted.Street2.ShouldBeNull();
    }

    [Fact]
    public void EncryptDoesNotChangeTheOriginal()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var address = AddressTestData.Create(Id);

        // Act
        _ = address.Encrypt(handler);

        // Assert
        address.Street1.ShouldBe("Main Street 1");
    }

    [Fact]
    public void DecryptRestoresTheOriginalValues()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var address = AddressTestData.Create(Id, street2: "Floor 2", co: "c/o Anna", state: string.Empty);

        // Act
        var decrypted = address.Encrypt(handler).Decrypt(handler);

        // Assert
        decrypted.CO.ShouldBe("c/o Anna");
        decrypted.Street1.ShouldBe("Main Street 1");
        decrypted.Street2.ShouldBe("Floor 2");
        decrypted.PostalCode.ShouldBe("12345");
        decrypted.City.ShouldBe("Stockholm");
        decrypted.State.ShouldBe(string.Empty);
        decrypted.Country.ShouldBe("Sweden");
    }

    [Fact]
    public void DecryptRejectsPlaintext()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var address = AddressTestData.Create(Id);

        // Act
        var action = () => address.Decrypt(handler);

        // Assert
        action.ShouldThrow<CryptographicException>();
    }

    [Fact]
    public void DecryptRejectsCiphertextMovedToAnotherAddress()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var encrypted = AddressTestData.Create(Id).Encrypt(handler);
        var moved = AddressTestData.Create(Guid.NewGuid(), street1: encrypted.Street1, postalCode: encrypted.PostalCode, city: encrypted.City, state: encrypted.State, country: encrypted.Country);

        // Act
        var action = () => moved.Decrypt(handler);

        // Assert
        action.ShouldThrow<CryptographicException>();
    }
}
