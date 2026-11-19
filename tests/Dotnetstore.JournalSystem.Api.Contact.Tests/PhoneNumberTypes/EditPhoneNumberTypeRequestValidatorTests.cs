using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumberTypes;

public sealed class EditPhoneNumberTypeRequestValidatorTests
{
    private static EditPhoneNumberTypeRequest ValidRequest() => new("Administrator", "Can do everything");

    [Fact]
    public void ValidateReturnsNoErrorsForValidRequest()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var errors = EditPhoneNumberTypeRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsEmptyDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = string.Empty };

        // Act
        var errors = EditPhoneNumberTypeRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsValuesAtMaximumLength()
    {
        // Arrange
        var request = new EditPhoneNumberTypeRequest(
            new string('n', DataSchemeConstants.PhoneNumberTypeNameMaxLength),
            new string('d', DataSchemeConstants.PhoneNumberTypeDescriptionMaxLength));

        // Act
        var errors = EditPhoneNumberTypeRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateReturnsErrorForNullRequest()
    {
        // Arrange & Act
        var errors = EditPhoneNumberTypeRequestValidator.Validate(null);

        // Assert
        errors.ShouldContainKey("request");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateReturnsErrorForMissingName(string? name)
    {
        // Arrange
        var request = ValidRequest() with { Name = name! };

        // Act
        var errors = EditPhoneNumberTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongName()
    {
        // Arrange
        var request = ValidRequest() with { Name = new string('n', DataSchemeConstants.PhoneNumberTypeNameMaxLength + 1) };

        // Act
        var errors = EditPhoneNumberTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForMissingDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = null! };

        // Act
        var errors = EditPhoneNumberTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Description"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = new string('d', DataSchemeConstants.PhoneNumberTypeDescriptionMaxLength + 1) };

        // Act
        var errors = EditPhoneNumberTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Description"]);
    }

    [Fact]
    public void ValidateReportsAllInvalidFieldsTogether()
    {
        // Arrange
        var request = new EditPhoneNumberTypeRequest("", new string('d', DataSchemeConstants.PhoneNumberTypeDescriptionMaxLength + 1));

        // Act
        var errors = EditPhoneNumberTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name", "Description"], ignoreOrder: true);
    }
}
