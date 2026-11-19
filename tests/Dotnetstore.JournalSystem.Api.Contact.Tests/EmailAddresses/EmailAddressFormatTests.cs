using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;

public sealed class EmailAddressFormatTests
{
    [Theory]
    [InlineData("anna@example.com")]
    [InlineData("anna.svensson+journal@sub.example.se")]
    [InlineData("A@B.CO")]
    public void IsValidAcceptsPlainEmailAddresses(string value)
    {
        // Arrange & Act
        var result = EmailAddressFormat.IsValid(value);

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("anna@")]
    [InlineData("@example.com")]
    [InlineData("anna@localhost")]
    [InlineData("Anna <anna@example.com>")]
    [InlineData(" anna@example.com")]
    [InlineData("anna@example.com ")]
    [InlineData("anna@@example.com")]
    public void IsValidRejectsEverythingElse(string? value)
    {
        // Arrange & Act
        var result = EmailAddressFormat.IsValid(value);

        // Assert
        result.ShouldBeFalse();
    }
}
