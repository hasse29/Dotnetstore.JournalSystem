using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;

public sealed class PhoneNumberFormatTests
{
    [Theory]
    [InlineData("+46 70 123 45 67")]
    [InlineData("0701234567")]
    [InlineData("(08) 123 456")]
    [InlineData("+1-202-555-0123")]
    [InlineData("12345")]
    [InlineData("123456789012345")]
    public void IsValidAcceptsPlainPhoneNumbers(string value)
    {
        // Arrange & Act
        var result = PhoneNumberFormat.IsValid(value);

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-number")]
    [InlineData("Call me 0701234567")]
    [InlineData(" 0701234567")]
    [InlineData("0701234567 ")]
    [InlineData("1234")]
    [InlineData("1234567890123456")]
    [InlineData("46+701234567")]
    [InlineData("070#1234567")]
    [InlineData("+")]
    public void IsValidRejectsEverythingElse(string? value)
    {
        // Arrange & Act
        var result = PhoneNumberFormat.IsValid(value);

        // Assert
        result.ShouldBeFalse();
    }
}
