using Dotnetstore.JournalSystem.Api.Organization.Users;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class PersonalIdentityNumberTests
{
    [Theory]
    [InlineData("19900101-1234", "199001011234")]
    [InlineData("900101+1234", "9001011234")]
    [InlineData(" 1990 0101 1234 ", "199001011234")]
    public void NormalizeStripsSeparatorsAndSpaces(string input, string expected)
    {
        // Arrange & Act
        var result = PersonalIdentityNumber.Normalize(input);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeReturnsNullForBlankInput(string? input)
    {
        // Arrange & Act
        var result = PersonalIdentityNumber.Normalize(input);

        // Assert
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData("19900101-1234")]
    [InlineData("9001011234")]
    public void IsValidAcceptsTenAndTwelveDigits(string input)
    {
        // Arrange & Act
        var result = PersonalIdentityNumber.IsValid(input);

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("19900101-12AB")]
    [InlineData("1990010112345")]
    [InlineData(null)]
    public void IsValidRejectsWrongLengthAndNonDigits(string? input)
    {
        // Arrange & Act
        var result = PersonalIdentityNumber.IsValid(input);

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData("199001011234", "19900101-****")]
    [InlineData("9001011234", "900101-****")]
    [InlineData("123", "****")]
    public void MaskOnlyRevealsTheBirthDatePart(string normalized, string expected)
    {
        // Arrange & Act
        var masked = PersonalIdentityNumber.Mask(normalized);

        // Assert
        masked.ShouldBe(expected);
    }
}
