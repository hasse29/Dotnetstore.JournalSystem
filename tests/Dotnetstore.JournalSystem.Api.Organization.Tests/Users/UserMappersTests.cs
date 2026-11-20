using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class UserMappersTests
{
    [Fact]
    public void ToUserResponseMapsAllPublicFields()
    {
        // Arrange
        var user = UserTestData.Create(Guid.NewGuid(), authProvider: AuthProvider.Entra, externalId: "oid-1");

        // Act
        var response = user.ToUserResponse();

        // Assert
        response.Id.ShouldBe(user.Id);
        response.FirstName.ShouldBe(user.FirstName);
        response.LastName.ShouldBe(user.LastName);
        response.Username.ShouldBe(user.Username);
        response.OwnCompanyId.ShouldBe(user.OwnCompanyId);
        response.RoleId.ShouldBe(user.RoleId);
        response.AuthProvider.ShouldBe(AuthProvider.Entra);
        response.IsLockedOut.ShouldBeFalse();
    }

    [Fact]
    public void UserResponseDoesNotExposeCredentials()
    {
        // Arrange & Act
        var properties = typeof(Dotnetstore.JournalSystem.Shared.Responses.Organization.UserResponse)
            .GetProperties()
            .Select(x => x.Name)
            .ToList();

        // Assert
        properties.ShouldNotContain("PasswordHash");
        properties.ShouldNotContain("SecurityStamp");
        properties.ShouldNotContain("ExternalId");
    }
}
