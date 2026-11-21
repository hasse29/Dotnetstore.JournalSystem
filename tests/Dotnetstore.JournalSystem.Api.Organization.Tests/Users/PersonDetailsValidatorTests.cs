using Dotnetstore.JournalSystem.Api.Organization.Users;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class PersonDetailsValidatorTests
{
    private static Dictionary<string, string[]> Run(DateOnly? dateOfBirth, string? number)
    {
        var errors = new Dictionary<string, string[]>();
        PersonDetailsValidator.Validate(errors, dateOfBirth, number);
        return errors;
    }

    [Fact]
    public void AcceptsMissingOptionalValues()
    {
        // Arrange & Act
        var errors = Run(null, null);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void AcceptsAValidDateAndNumber()
    {
        // Arrange & Act
        var errors = Run(new DateOnly(1990, 1, 1), "19900101-1234");

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void AcceptsAnEmptyNumberToClearIt()
    {
        // Arrange & Act
        var errors = Run(null, string.Empty);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void RejectsADateInTheFuture()
    {
        // Arrange & Act
        var errors = Run(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(2), null);

        // Assert
        errors.ShouldContainKey("DateOfBirth");
    }

    [Fact]
    public void RejectsADateBefore1900()
    {
        // Arrange & Act
        var errors = Run(new DateOnly(1899, 12, 31), null);

        // Assert
        errors.ShouldContainKey("DateOfBirth");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("19900101-12AB")]
    public void RejectsAMalformedNumber(string number)
    {
        // Arrange & Act
        var errors = Run(null, number);

        // Assert
        errors.ShouldContainKey("SocialSecurityNumber");
    }
}
