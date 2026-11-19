using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.AddressTypes;

public sealed class EditAddressTypeRequestValidatorTests
{
    private static EditAddressTypeRequest ValidRequest() => new("Administrator", "Can do everything");

    [Fact]
    public void ValidateReturnsNoErrorsForValidRequest()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var errors = EditAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsEmptyDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = string.Empty };

        // Act
        var errors = EditAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsValuesAtMaximumLength()
    {
        // Arrange
        var request = new EditAddressTypeRequest(
            new string('n', DataSchemeConstants.AddressTypeNameMaxLength),
            new string('d', DataSchemeConstants.AddressTypeDescriptionMaxLength));

        // Act
        var errors = EditAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateReturnsErrorForNullRequest()
    {
        // Arrange & Act
        var errors = EditAddressTypeRequestValidator.Validate(null);

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
        var errors = EditAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongName()
    {
        // Arrange
        var request = ValidRequest() with { Name = new string('n', DataSchemeConstants.AddressTypeNameMaxLength + 1) };

        // Act
        var errors = EditAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForMissingDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = null! };

        // Act
        var errors = EditAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Description"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = new string('d', DataSchemeConstants.AddressTypeDescriptionMaxLength + 1) };

        // Act
        var errors = EditAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Description"]);
    }

    [Fact]
    public void ValidateReportsAllInvalidFieldsTogether()
    {
        // Arrange
        var request = new EditAddressTypeRequest("", new string('d', DataSchemeConstants.AddressTypeDescriptionMaxLength + 1));

        // Act
        var errors = EditAddressTypeRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name", "Description"], ignoreOrder: true);
    }
}
