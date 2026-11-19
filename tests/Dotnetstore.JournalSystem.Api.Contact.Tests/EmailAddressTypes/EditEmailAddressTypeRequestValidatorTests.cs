using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddressTypes;

public sealed class EditEmailAddressTypeRequestValidatorTests
{
    private static EditEmailAddressTypeRequest ValidRequest() => new("Administrator", "Can do everything");

    [Fact]
    public void ValidateReturnsNoErrorsForValidRequest()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var errors = EditEmailAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsEmptyDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = string.Empty };

        // Act
        var errors = EditEmailAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsValuesAtMaximumLength()
    {
        // Arrange
        var request = new EditEmailAddressTypeRequest(
            new string('n', DataSchemeConstants.EmailAddressTypeNameMaxLength),
            new string('d', DataSchemeConstants.EmailAddressTypeDescriptionMaxLength));

        // Act
        var errors = EditEmailAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateReturnsErrorForNullRequest()
    {
        // Arrange & Act
        var errors = EditEmailAddressTypeRequestValidator.Validate(null);

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
        var errors = EditEmailAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongName()
    {
        // Arrange
        var request = ValidRequest() with { Name = new string('n', DataSchemeConstants.EmailAddressTypeNameMaxLength + 1) };

        // Act
        var errors = EditEmailAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForMissingDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = null! };

        // Act
        var errors = EditEmailAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Description"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = new string('d', DataSchemeConstants.EmailAddressTypeDescriptionMaxLength + 1) };

        // Act
        var errors = EditEmailAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Description"]);
    }

    [Fact]
    public void ValidateReportsAllInvalidFieldsTogether()
    {
        // Arrange
        var request = new EditEmailAddressTypeRequest("", new string('d', DataSchemeConstants.EmailAddressTypeDescriptionMaxLength + 1));

        // Act
        var errors = EditEmailAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name", "Description"], ignoreOrder: true);
    }
}
