using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class UserBuilderTests
{
    [Fact]
    public void CreateWithCreateValuesBuildsUserFromRequest()
    {
        // Arrange
        var request = UserTestData.CreateRequest();
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var user = UserBuilder.Create().WithCreateValues(request, "hash", null, userId).Build();

        // Assert
        user.Id.ShouldNotBe(Guid.Empty);
        user.FirstName.ShouldBe("Anna");
        user.LastName.ShouldBe("Svensson");
        user.Username.ShouldBe("anna@example.com");
        user.PasswordHash.ShouldBe("hash");
        user.SecurityStamp.ShouldNotBeNullOrWhiteSpace();
        user.OwnCompanyId.ShouldBe(request.OwnCompanyId);
        user.RoleId.ShouldBe(request.RoleId);
        user.AuthProvider.ShouldBe(AuthProvider.Local);
        user.ExternalId.ShouldBeNull();
        user.AccountIsApproved.ShouldBeTrue();
        user.IsGdpr.ShouldBeTrue();
        user.CreatedBy.ShouldBe(userId);
        user.CreatedDate.ShouldBeInRange(before, DateTime.UtcNow);
    }

    [Fact]
    public void CreateWithCreateValuesStoresExternalIdForEntraUsers()
    {
        // Arrange
        var request = UserTestData.CreateRequest(authProvider: AuthProvider.Entra, password: null);

        // Act
        var user = UserBuilder.Create().WithCreateValues(request, string.Empty, "entra-object-id", null).Build();

        // Assert
        user.AuthProvider.ShouldBe(AuthProvider.Entra);
        user.ExternalId.ShouldBe("entra-object-id");
    }

    [Fact]
    public void CreateWithCreateValuesGeneratesNewIdAndStampEachTime()
    {
        // Arrange
        var request = UserTestData.CreateRequest();

        // Act
        var first = UserBuilder.Create().WithCreateValues(request, "hash", null, null).Build();
        var second = UserBuilder.Create().WithCreateValues(request, "hash", null, null).Build();

        // Assert
        first.Id.ShouldNotBe(second.Id);
        first.SecurityStamp.ShouldNotBe(second.SecurityStamp);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithCreateValuesRejectsBlankUsername(string username)
    {
        // Arrange
        var request = UserTestData.CreateRequest(username);

        // Act
        var act = () => UserBuilder.Create().WithCreateValues(request, "hash", null, null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsEmptyRoleId()
    {
        // Arrange
        var request = UserTestData.CreateRequest() with { RoleId = Guid.Empty };

        // Act
        var act = () => UserBuilder.Create().WithCreateValues(request, "hash", null, null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsTooLongFirstName()
    {
        // Arrange
        var request = UserTestData.CreateRequest() with { FirstName = new string('a', DataSchemeConstants.UserNameMaxLength + 1) };

        // Act
        var act = () => UserBuilder.Create().WithCreateValues(request, "hash", null, null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsEmptyUserId()
    {
        // Arrange
        var request = UserTestData.CreateRequest();

        // Act
        var act = () => UserBuilder.Create().WithCreateValues(request, "hash", null, Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsCredentialsAndIdentity()
    {
        // Arrange
        var existing = UserTestData.Create(Guid.NewGuid(), authProvider: AuthProvider.Entra, externalId: "oid-1");
        var request = new EditUserRequest("Bo", "Karl", "Nilsson", Guid.NewGuid(), Guid.NewGuid());
        var userId = Guid.NewGuid();

        // Act
        var user = UserBuilder.Update().WithUpdateValues(existing, request, userId).Build();

        // Assert
        user.Id.ShouldBe(existing.Id);
        user.Username.ShouldBe(existing.Username);
        user.PasswordHash.ShouldBe(existing.PasswordHash);
        user.SecurityStamp.ShouldBe(existing.SecurityStamp);
        user.AuthProvider.ShouldBe(AuthProvider.Entra);
        user.ExternalId.ShouldBe("oid-1");
        user.CreatedDate.ShouldBe(existing.CreatedDate);
        user.FirstName.ShouldBe("Bo");
        user.MiddleName.ShouldBe("Karl");
        user.LastName.ShouldBe("Nilsson");
        user.OwnCompanyId.ShouldBe(request.OwnCompanyId);
        user.RoleId.ShouldBe(request.RoleId);
        user.LastUpdatedBy.ShouldBe(userId);
        user.LastUpdatedDate.ShouldNotBeNull();
    }

    [Fact]
    public void UpdateWithUpdateValuesRejectsBlankLastName()
    {
        // Arrange
        var existing = UserTestData.Create(Guid.NewGuid());
        var request = new EditUserRequest("Bo", null, " ", Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = () => UserBuilder.Update().WithUpdateValues(existing, request, null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }
}
