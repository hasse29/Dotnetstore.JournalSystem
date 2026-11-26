using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;

public sealed class EditRoleRequestValidatorTests
{
    private static EditRoleRequest ValidRequest() => new("Administrator", "Can do everything");

    [Fact]
    public void ValidateReturnsNoErrorsForValidRequest()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var errors = EditRoleRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsEmptyDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = string.Empty };

        // Act
        var errors = EditRoleRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateAcceptsValuesAtMaximumLength()
    {
        // Arrange
        var request = new EditRoleRequest(
            new string('n', DataSchemeConstants.RoleNameMaxLength),
            new string('d', DataSchemeConstants.RoleDescriptionMaxLength));

        // Act
        var errors = EditRoleRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateReturnsErrorForNullRequest()
    {
        // Arrange & Act
        var errors = EditRoleRequestValidator.Validate(null);

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
        var errors = EditRoleRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongName()
    {
        // Arrange
        var request = ValidRequest() with { Name = new string('n', DataSchemeConstants.RoleNameMaxLength + 1) };

        // Act
        var errors = EditRoleRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name"]);
    }

    [Fact]
    public void ValidateReturnsErrorForMissingDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = null! };

        // Act
        var errors = EditRoleRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Description"]);
    }

    [Fact]
    public void ValidateReturnsErrorForTooLongDescription()
    {
        // Arrange
        var request = ValidRequest() with { Description = new string('d', DataSchemeConstants.RoleDescriptionMaxLength + 1) };

        // Act
        var errors = EditRoleRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Description"]);
    }

    [Fact]
    public void ValidateReportsAllInvalidFieldsTogether()
    {
        // Arrange
        var request = new EditRoleRequest("", new string('d', DataSchemeConstants.RoleDescriptionMaxLength + 1));

        // Act
        var errors = EditRoleRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(["Name", "Description"], ignoreOrder: true);
    }

    [Fact]
    public void KnownPermissionsAreAccepted()
    {
        // Arrange
        var request = new EditRoleRequest("Nurse", "Description", [Permissions.Users.View]);

        // Act
        var errors = EditRoleRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void UnknownPermissionsAreRefused()
    {
        // Arrange
        var request = new EditRoleRequest("Nurse", "Description", [Permissions.Users.View, "everything:all"]);

        // Act
        var errors = EditRoleRequestValidator.Validate(request);

        // Assert
        errors.ShouldContainKey(nameof(EditRoleRequest.Permissions));
    }
}
