using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class UserRequestValidatorTests
{
    [Fact]
    public void ValidateCreateAcceptsValidLocalRequest()
    {
        // Arrange
        var request = UserTestData.CreateRequest();

        // Act
        var errors = UserRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateCreateAcceptsEntraRequestWithoutPassword()
    {
        // Arrange
        var request = UserTestData.CreateRequest(authProvider: AuthProvider.Entra, password: null);

        // Act
        var errors = UserRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateCreateRejectsNullRequest()
    {
        // Arrange & Act
        var errors = UserRequestValidator.Validate((CreateUserRequest?)null);

        // Assert
        errors.ShouldContainKey("request");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("Anna <anna@example.com>")]
    public void ValidateCreateRejectsInvalidUsername(string username)
    {
        // Arrange
        var request = UserTestData.CreateRequest(username);

        // Act
        var errors = UserRequestValidator.Validate(request);

        // Assert
        errors.ShouldContainKey(nameof(CreateUserRequest.Username));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("short")]
    public void ValidateCreateRejectsMissingOrShortPasswordForLocalUsers(string? password)
    {
        // Arrange
        var request = UserTestData.CreateRequest(password: password);

        // Act
        var errors = UserRequestValidator.Validate(request);

        // Assert
        errors.ShouldContainKey(nameof(CreateUserRequest.Password));
    }

    [Fact]
    public void ValidateCreateRejectsPasswordForEntraUsers()
    {
        // Arrange
        var request = UserTestData.CreateRequest(authProvider: AuthProvider.Entra, password: "a-long-enough-password");

        // Act
        var errors = UserRequestValidator.Validate(request);

        // Assert
        errors.ShouldContainKey(nameof(CreateUserRequest.Password));
    }

    [Fact]
    public void ValidateCreateRejectsUndefinedAuthProvider()
    {
        // Arrange
        var request = UserTestData.CreateRequest(authProvider: (AuthProvider)99);

        // Act
        var errors = UserRequestValidator.Validate(request);

        // Assert
        errors.ShouldContainKey(nameof(CreateUserRequest.AuthProvider));
    }

    [Fact]
    public void ValidateCreateRejectsEmptyIdsAndBlankNames()
    {
        // Arrange
        var request = new CreateUserRequest(" ", " ", new string('a', 26), "anna@example.com", Guid.Empty, Guid.Empty, AuthProvider.Local, "a-long-enough-password");

        // Act
        var errors = UserRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(
            [
                nameof(CreateUserRequest.FirstName),
                nameof(CreateUserRequest.MiddleName),
                nameof(CreateUserRequest.LastName),
                nameof(CreateUserRequest.OwnCompanyId),
                nameof(CreateUserRequest.RoleId)
            ],
            ignoreOrder: true);
    }

    [Fact]
    public void ValidateEditAcceptsValidRequest()
    {
        // Arrange
        var request = new EditUserRequest("Bo", null, "Nilsson", Guid.NewGuid(), Guid.NewGuid());

        // Act
        var errors = UserRequestValidator.Validate(request);

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateEditRejectsNullRequest()
    {
        // Arrange & Act
        var errors = UserRequestValidator.Validate((EditUserRequest?)null);

        // Assert
        errors.ShouldContainKey("request");
    }

    [Fact]
    public void ValidateEditRejectsBlankFirstNameAndEmptyIds()
    {
        // Arrange
        var request = new EditUserRequest("", null, "Nilsson", Guid.Empty, Guid.Empty);

        // Act
        var errors = UserRequestValidator.Validate(request);

        // Assert
        errors.Keys.ShouldBe(
            [nameof(EditUserRequest.FirstName), nameof(EditUserRequest.OwnCompanyId), nameof(EditUserRequest.RoleId)],
            ignoreOrder: true);
    }
}
