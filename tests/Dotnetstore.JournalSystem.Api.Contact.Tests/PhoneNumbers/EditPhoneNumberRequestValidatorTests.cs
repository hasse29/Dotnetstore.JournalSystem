using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;

public sealed class EditPhoneNumberRequestValidatorTests
{
    private static EditPhoneNumberRequest ValidRequest() =>
        new(PhoneNumberTestData.PhoneNumberTypeId, PhoneNumberTestData.ObjectId, "+46 70 123 45 67");

    [Fact]
    public void ValidateReturnsNoErrorsForValidRequest()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var errors = EditPhoneNumberRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsNumberAtMaximumLength()
    {
        // Arrange
        var request = ValidRequest() with { Number = "+46 70 123 45 67" + new string('-', DataSchemeConstants.PhoneNumberMaxLength - 16) };

        // Act
        var errors = EditPhoneNumberRequestValidator.Validate(request);

        // Assert
        request.Number.Length.ShouldBe(DataSchemeConstants.PhoneNumberMaxLength);
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateReturnsErrorForNullRequest()
    {
        // Arrange & Act
        var errors = EditPhoneNumberRequestValidator.Validate(null);

        // Assert
        errors.ShouldContainKey("request");
    }

    [Fact]
    public void ValidateReturnsErrorForEmptyPhoneNumberTypeId()
    {
        // Arrange
        var request = ValidRequest() with { PhoneNumberTypeId = Guid.Empty };

        // Act
        var errors = EditPhoneNumberRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["PhoneNumberTypeId"]);
    }

    [Fact]
    public void ValidateReturnsErrorForEmptyObjectId()
    {
        // Arrange
        var request = ValidRequest() with { ObjectId = Guid.Empty };

        // Act
        var errors = EditPhoneNumberRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["ObjectId"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateReturnsErrorForMissingAddress(string? number)
    {
        // Arrange
        var request = ValidRequest() with { Number = number! };

        // Act
        var errors = EditPhoneNumberRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Number"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongNumber()
    {
        // Arrange
        var request = ValidRequest() with { Number = new string('1', DataSchemeConstants.PhoneNumberMaxLength + 1) };

        // Act
        var errors = EditPhoneNumberRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Number"]);
        errors["Number"].ShouldHaveSingleItem().ShouldContain("at most");
    }

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("1234")]
    [InlineData("Call me 0701234567")]
    public void ValidateReturnsErrorForInvalidFormat(string number)
    {
        // Arrange
        var request = ValidRequest() with { Number = number };

        // Act
        var errors = EditPhoneNumberRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Number"]);
        errors["Number"].ShouldHaveSingleItem().ShouldContain("valid phone number");
    }

    [Fact]
    public void ValidateReportsAllInvalidFieldsTogether()
    {
        // Arrange
        var request = new EditPhoneNumberRequest(Guid.Empty, Guid.Empty, "");

        // Act
        var errors = EditPhoneNumberRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["PhoneNumberTypeId", "ObjectId", "Number"], ignoreOrder: true);
    }
}
