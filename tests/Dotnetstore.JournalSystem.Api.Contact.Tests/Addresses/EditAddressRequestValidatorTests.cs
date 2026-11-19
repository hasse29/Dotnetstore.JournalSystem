using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Addresses;

public sealed class EditAddressRequestValidatorTests
{
    private static EditAddressRequest ValidRequest() =>
        new(AddressTestData.AddressTypeId, AddressTestData.ObjectId, "c/o Anna", "Main Street 1", "Floor 2", "12345", "Stockholm", "Stockholm County", "Sweden");

    [Fact]
    public void ValidateReturnsNoErrorsForValidRequest()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var errors = EditAddressRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsMissingCOAndStreet2AndEmptyState()
    {
        // Arrange
        var request = ValidRequest() with { CO = null, Street2 = null, State = string.Empty };

        // Act
        var errors = EditAddressRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsValuesAtMaximumLength()
    {
        // Arrange
        var request = new EditAddressRequest(
            AddressTestData.AddressTypeId,
            AddressTestData.ObjectId,
            new string('o', DataSchemeConstants.AddressCOMaxLength),
            new string('a', DataSchemeConstants.AddressStreetMaxLength),
            new string('b', DataSchemeConstants.AddressStreetMaxLength),
            new string('c', DataSchemeConstants.AddressPostalCodeMaxLength),
            new string('d', DataSchemeConstants.AddressCityMaxLength),
            new string('e', DataSchemeConstants.AddressStateMaxLength),
            new string('f', DataSchemeConstants.AddressCountryMaxLength));

        // Act
        var errors = EditAddressRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateReturnsErrorForNullRequest()
    {
        // Arrange & Act
        var errors = EditAddressRequestValidator.Validate(null);

        // Assert
        errors.ShouldContainKey("request");
    }

    [Fact]
    public void ValidateReturnsErrorForEmptyAddressTypeId()
    {
        // Arrange
        var request = ValidRequest() with { AddressTypeId = Guid.Empty };

        // Act
        var errors = EditAddressRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["AddressTypeId"]);
    }

    [Fact]
    public void ValidateReturnsErrorForEmptyObjectId()
    {
        // Arrange
        var request = ValidRequest() with { ObjectId = Guid.Empty };

        // Act
        var errors = EditAddressRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["ObjectId"]);
    }

    [Theory]
    [InlineData("Street1")]
    [InlineData("PostalCode")]
    [InlineData("City")]
    [InlineData("Country")]
    public void ValidateReturnsErrorForMissingRequiredText(string field)
    {
        foreach (var value in new string?[] { "", "   ", null })
        {
            // Arrange
            var request = field switch
            {
                "Street1" => ValidRequest() with { Street1 = value! },
                "PostalCode" => ValidRequest() with { PostalCode = value! },
                "City" => ValidRequest() with { City = value! },
                _ => ValidRequest() with { Country = value! }
            };

            // Act
            var errors = EditAddressRequestValidator.Validate(request);

            // Assert
            errors.Keys.ShouldBe([field]);
        }
    }

    [Fact]
    public void ValidateReturnsErrorForNullState()
    {
        // Arrange
        var request = ValidRequest() with { State = null! };

        // Act
        var errors = EditAddressRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["State"]);
    }

    [Theory]
    [InlineData("Street1", DataSchemeConstants.AddressStreetMaxLength)]
    [InlineData("CO", DataSchemeConstants.AddressCOMaxLength)]
    [InlineData("Street2", DataSchemeConstants.AddressStreetMaxLength)]
    [InlineData("PostalCode", DataSchemeConstants.AddressPostalCodeMaxLength)]
    [InlineData("City", DataSchemeConstants.AddressCityMaxLength)]
    [InlineData("State", DataSchemeConstants.AddressStateMaxLength)]
    [InlineData("Country", DataSchemeConstants.AddressCountryMaxLength)]
    public void ValidateReturnsErrorForTooLongText(string field, int maxLength)
    {
        // Arrange
        var tooLong = new string('x', maxLength + 1);
        var request = field switch
        {
            "Street1" => ValidRequest() with { Street1 = tooLong },
            "CO" => ValidRequest() with { CO = tooLong },
            "Street2" => ValidRequest() with { Street2 = tooLong },
            "PostalCode" => ValidRequest() with { PostalCode = tooLong },
            "City" => ValidRequest() with { City = tooLong },
            "State" => ValidRequest() with { State = tooLong },
            _ => ValidRequest() with { Country = tooLong }
        };

        // Act
        var errors = EditAddressRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe([field]);
    }

    [Fact]
    public void ValidateReportsAllInvalidFieldsTogether()
    {
        // Arrange
        var request = new EditAddressRequest(Guid.Empty, Guid.Empty, null, "", null, "", "", null!, "");

        // Act
        var errors = EditAddressRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["AddressTypeId", "ObjectId", "Street1", "PostalCode", "City", "State", "Country"], ignoreOrder: true);
    }
}
