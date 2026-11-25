using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;
using Dotnetstore.JournalSystem.Shared.Constants;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies.Images;

public sealed class OwnCompanyImageValidatorTests
{
    public static TheoryData<byte[], string> SupportedImages => new()
    {
        { [0xFF, 0xD8, 0xFF, 0xE0], "image/jpeg" },
        { [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], "image/png" },
        { [.. "RIFF"u8, 0, 0, 0, 0, .. "WEBP"u8], "image/webp" }
    };

    [Theory]
    [MemberData(nameof(SupportedImages))]
    public void SupportedImagesAreAcceptedAndRecognised(byte[] content, string contentType)
    {
        // Arrange

        // Act
        var errors = OwnCompanyImageValidator.Validate(content);
        var detected = OwnCompanyImageValidator.DetectContentType(content);

        // Assert
        errors.ShouldBeEmpty();
        detected.ShouldBe(contentType);
    }

    public static TheoryData<byte[]> UnsupportedContent => new()
    {
        "<svg/>"u8.ToArray(),
        "GIF89a"u8.ToArray(),
        "RIFF0000WAVE"u8.ToArray(),
        "RIFF"u8.ToArray(),
        new byte[] { 0xFF, 0xD8 }
    };

    [Theory]
    [MemberData(nameof(UnsupportedContent))]
    public void OtherContentIsRejected(byte[] content)
    {
        // Arrange

        // Act
        var errors = OwnCompanyImageValidator.Validate(content);

        // Assert
        errors[OwnCompanyImageConstants.FormFieldName].ShouldHaveSingleItem().ShouldContain("JPEG, PNG and WebP");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AMissingOrEmptyFileIsRejected(bool missing)
    {
        // Arrange
        byte[]? content = missing ? null : [];

        // Act
        var errors = OwnCompanyImageValidator.Validate(content);

        // Assert
        errors.ShouldContainKey(OwnCompanyImageConstants.FormFieldName);
    }

    [Fact]
    public void AnImageOverTheSizeLimitIsRejected()
    {
        // Arrange
        var content = new byte[OwnCompanyImageConstants.MaxSizeBytes + 1];
        content[0] = 0xFF;
        content[1] = 0xD8;
        content[2] = 0xFF;

        // Act
        var errors = OwnCompanyImageValidator.Validate(content);

        // Assert
        errors[OwnCompanyImageConstants.FormFieldName].ShouldHaveSingleItem().ShouldContain("2 MB");
    }
}
