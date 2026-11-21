using System.Security.Cryptography;
using Dotnetstore.JournalSystem.Api.SharedKernel.Security;
using Dotnetstore.JournalSystem.Api.SharedKernel.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.SharedKernel.Tests.Security;

public sealed class FieldEncryptionTests
{
    private static readonly Guid RecordId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void EncryptFieldProducesCiphertextThatDecryptsBack()
    {
        // Arrange
        var handler = TestEncryption.Create();

        // Act
        var encrypted = handler.EncryptField("Main Street 1", RecordId, "Street1");
        var decrypted = handler.DecryptField(encrypted, RecordId, "Street1");

        // Assert
        encrypted.ShouldNotBeNull().ShouldStartWith("dse1.");
        encrypted.ShouldNotContain("Main Street");
        decrypted.ShouldBe("Main Street 1");
    }

    [Fact]
    public void EncryptFieldProducesDifferentCiphertextEachTime()
    {
        // Arrange
        var handler = TestEncryption.Create();

        // Act
        var first = handler.EncryptField("same", RecordId, "Street1");
        var second = handler.EncryptField("same", RecordId, "Street1");

        // Assert
        first.ShouldNotBe(second);
    }

    [Fact]
    public void EncryptAndDecryptFieldPassNullThrough()
    {
        // Arrange
        var handler = TestEncryption.Create();

        // Act
        var encrypted = handler.EncryptField(null, RecordId, "CO");
        var decrypted = handler.DecryptField(null, RecordId, "CO");

        // Assert
        encrypted.ShouldBeNull();
        decrypted.ShouldBeNull();
    }

    [Fact]
    public void EncryptFieldRoundTripsAnEmptyString()
    {
        // Arrange
        var handler = TestEncryption.Create();

        // Act
        var encrypted = handler.EncryptField(string.Empty, RecordId, "State");
        var decrypted = handler.DecryptField(encrypted, RecordId, "State");

        // Assert
        encrypted.ShouldNotBeNullOrEmpty();
        decrypted.ShouldBe(string.Empty);
    }

    [Fact]
    public void DecryptFieldRejectsCiphertextFromAnotherRecord()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var encrypted = handler.EncryptField("secret", RecordId, "Street1");

        // Act
        var action = () => handler.DecryptField(encrypted, Guid.NewGuid(), "Street1");

        // Assert
        action.ShouldThrow<CryptographicException>();
    }

    [Fact]
    public void DecryptFieldRejectsCiphertextFromAnotherField()
    {
        // Arrange
        var handler = TestEncryption.Create();
        var encrypted = handler.EncryptField("secret", RecordId, "Street1");

        // Act
        var action = () => handler.DecryptField(encrypted, RecordId, "City");

        // Assert
        action.ShouldThrow<CryptographicException>();
    }

    [Fact]
    public void DecryptFieldRejectsPlaintext()
    {
        // Arrange
        var handler = TestEncryption.Create();

        // Act
        var action = () => handler.DecryptField("not encrypted", RecordId, "Street1");

        // Assert
        action.ShouldThrow<CryptographicException>();
    }

    [Fact]
    public void DecryptFieldRejectsCiphertextMadeWithAnUnknownKey()
    {
        // Arrange
        var foreign = TestEncryption.Create("other", Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
        var encrypted = foreign.EncryptField("secret", RecordId, "Street1");

        // Act
        var action = () => TestEncryption.Create().DecryptField(encrypted, RecordId, "Street1");

        // Assert
        action.ShouldThrow<CryptographicException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(100)]
    [InlineData(254)]
    public void EncryptedMaxLengthFitsTheLongestCiphertextForMultiByteText(int plaintextMaxLength)
    {
        // Arrange
        var handler = TestEncryption.Create(new string('k', 32), TestEncryption.Key);
        var worstCase = new string('€', plaintextMaxLength);

        // Act
        var encrypted = handler.EncryptField(worstCase, RecordId, "Street1");

        // Assert
        encrypted.ShouldNotBeNull().Length.ShouldBeLessThanOrEqualTo(FieldEncryption.EncryptedMaxLength(plaintextMaxLength));
    }

    [Fact]
    public void EncryptedMaxLengthGrowsWithThePlaintextLimit()
    {
        // Arrange & Act
        var small = FieldEncryption.EncryptedMaxLength(10);
        var large = FieldEncryption.EncryptedMaxLength(100);

        // Assert
        small.ShouldBeGreaterThan(10);
        large.ShouldBeGreaterThan(small);
    }

    [Fact]
    public void EncryptFieldRejectsBlankFieldName()
    {
        // Arrange
        var handler = TestEncryption.Create();

        // Act
        var action = () => handler.EncryptField("x", RecordId, " ");

        // Assert
        action.ShouldThrow<ArgumentException>();
    }
}
