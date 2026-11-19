using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.WorkGroups;

public sealed class EditWorkGroupRequestValidatorTests
{
    private static EditWorkGroupRequest ValidRequest() => new("Administrator", "Can do everything");

    [Fact]
    public void ValidateReturnsNoErrorsForValidRequest()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var errors = EditWorkGroupRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsEmptyDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = string.Empty };

        // Act
        var errors = EditWorkGroupRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsValuesAtMaximumLength()
    {
        // Arrange
        var request = new EditWorkGroupRequest(
            new string('n', DataSchemeConstants.WorkGroupNameMaxLength),
            new string('d', DataSchemeConstants.WorkGroupDescriptionMaxLength));

        // Act
        var errors = EditWorkGroupRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateReturnsErrorForNullRequest()
    {
        // Arrange & Act
        var errors = EditWorkGroupRequestValidator.Validate(null);

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
        var errors = EditWorkGroupRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongName()
    {
        // Arrange
        var request = ValidRequest() with { Name = new string('n', DataSchemeConstants.WorkGroupNameMaxLength + 1) };

        // Act
        var errors = EditWorkGroupRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForMissingDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = null! };

        // Act
        var errors = EditWorkGroupRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Description"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = new string('d', DataSchemeConstants.WorkGroupDescriptionMaxLength + 1) };

        // Act
        var errors = EditWorkGroupRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Description"]);
    }

    [Fact]
    public void ValidateReportsAllInvalidFieldsTogether()
    {
        // Arrange
        var request = new EditWorkGroupRequest("", new string('d', DataSchemeConstants.WorkGroupDescriptionMaxLength + 1));

        // Act
        var errors = EditWorkGroupRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name", "Description"], ignoreOrder: true);
    }
}
