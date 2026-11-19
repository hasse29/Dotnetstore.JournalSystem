using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;

public sealed class EditOwnCompanyRequestValidatorTests
{
    private static EditOwnCompanyRequest ValidRequest() =>
        new("Dotnetstore AB", "Description", null, new Uri("https://example.com"), "556677-8899", "SE556677889901");

    [Fact]
    public void ValidateReturnsNoErrorsForValidRequest()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var errors = EditOwnCompanyRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateReturnsNoErrorsForOnlyRequiredValues()
    {
        // Arrange
        var request = new EditOwnCompanyRequest("A", null, null, null, null, null);

        // Act
        var errors = EditOwnCompanyRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsValuesAtMaximumLength()
    {
        // Arrange
        var request = new EditOwnCompanyRequest(
            new string('n', DataSchemeConstants.OwnCompanyNameMaxLength),
            new string('d', DataSchemeConstants.OwnCompanyDescriptionMaxLength),
            null,
            null,
            new string('c', DataSchemeConstants.OwnCompanyCorporateNumberMaxLength),
            new string('t', DataSchemeConstants.OwnCompanyTaxIdMaxLength));

        // Act
        var errors = EditOwnCompanyRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateReturnsErrorForNullRequest()
    {
        // Arrange & Act
        var errors = EditOwnCompanyRequestValidator.Validate(null);

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
        var errors = EditOwnCompanyRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongName()
    {
        // Arrange
        var request = ValidRequest() with { Name = new string('n', DataSchemeConstants.OwnCompanyNameMaxLength + 1) };

        // Act
        var errors = EditOwnCompanyRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = new string('d', DataSchemeConstants.OwnCompanyDescriptionMaxLength + 1) };

        // Act
        var errors = EditOwnCompanyRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Description"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongCorporateNumber()
    {
        // Arrange
        var request = ValidRequest() with { CorporateNumber = new string('c', DataSchemeConstants.OwnCompanyCorporateNumberMaxLength + 1) };

        // Act
        var errors = EditOwnCompanyRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["CorporateNumber"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongTaxId()
    {
        // Arrange
        var request = ValidRequest() with { TaxId = new string('t', DataSchemeConstants.OwnCompanyTaxIdMaxLength + 1) };

        // Act
        var errors = EditOwnCompanyRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["TaxId"]);
    }

    [Fact]
    public void ValidateReportsAllInvalidFieldsTogether()
    {
        // Arrange
        var request = new EditOwnCompanyRequest(
            "",
            new string('d', DataSchemeConstants.OwnCompanyDescriptionMaxLength + 1),
            null,
            null,
            null,
            new string('t', DataSchemeConstants.OwnCompanyTaxIdMaxLength + 1));

        // Act
        var errors = EditOwnCompanyRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name", "Description", "TaxId"], ignoreOrder: true);
    }
}
