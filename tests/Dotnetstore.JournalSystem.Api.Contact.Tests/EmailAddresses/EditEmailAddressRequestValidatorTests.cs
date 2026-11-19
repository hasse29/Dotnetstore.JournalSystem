using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;

public sealed class EditEmailAddressRequestValidatorTests
{
    private static EditEmailAddressRequest ValidRequest() =>
        new(EmailAddressTestData.EmailAddressTypeId, EmailAddressTestData.ObjectId, "anna@example.com");

    [Fact]
    public void ValidateReturnsNoErrorsForValidRequest()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var errors = EditEmailAddressRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsAddressAtMaximumLength()
    {
        // Arrange
        var local = new string('a', 64);
        var domain = new string('d', DataSchemeConstants.EmailAddressMaxLength - 64 - 1 - 4) + ".com";
        var request = ValidRequest() with { Address = $"{local}@{domain}" };

        // Act
        var errors = EditEmailAddressRequestValidator.Validate(request);

        // Assert
        request.Address.Length.ShouldBe(DataSchemeConstants.EmailAddressMaxLength);
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateReturnsErrorForNullRequest()
    {
        // Arrange & Act
        var errors = EditEmailAddressRequestValidator.Validate(null);

        // Assert
        errors.ShouldContainKey("request");
    }

    [Fact]
    public void ValidateReturnsErrorForEmptyEmailAddressTypeId()
    {
        // Arrange
        var request = ValidRequest() with { EmailAddressTypeId = Guid.Empty };

        // Act
        var errors = EditEmailAddressRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["EmailAddressTypeId"]);
    }

    [Fact]
    public void ValidateReturnsErrorForEmptyObjectId()
    {
        // Arrange
        var request = ValidRequest() with { ObjectId = Guid.Empty };

        // Act
        var errors = EditEmailAddressRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["ObjectId"]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ValidateReturnsErrorForMissingAddress(string? address)
    {
        // Arrange
        var request = ValidRequest() with { Address = address! };

        // Act
        var errors = EditEmailAddressRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Address"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongAddress()
    {
        // Arrange
        var request = ValidRequest() with { Address = new string('a', DataSchemeConstants.EmailAddressMaxLength) + "@example.com" };

        // Act
        var errors = EditEmailAddressRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Address"]);
        errors["Address"].ShouldHaveSingleItem().ShouldContain("at most");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("anna@localhost")]
    [InlineData("Anna <anna@example.com>")]
    public void ValidateReturnsErrorForInvalidFormat(string address)
    {
        // Arrange
        var request = ValidRequest() with { Address = address };

        // Act
        var errors = EditEmailAddressRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Address"]);
        errors["Address"].ShouldHaveSingleItem().ShouldContain("valid email address");
    }

    [Fact]
    public void ValidateReportsAllInvalidFieldsTogether()
    {
        // Arrange
        var request = new EditEmailAddressRequest(Guid.Empty, Guid.Empty, "");

        // Act
        var errors = EditEmailAddressRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["EmailAddressTypeId", "ObjectId", "Address"], ignoreOrder: true);
    }
}
