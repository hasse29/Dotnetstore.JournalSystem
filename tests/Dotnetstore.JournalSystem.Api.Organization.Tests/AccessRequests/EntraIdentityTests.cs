using System.Security.Claims;
using Dotnetstore.JournalSystem.Api.Organization.AccessRequests;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.AccessRequests;

public sealed class EntraIdentityTests
{
    private static ClaimsPrincipal Principal(params (string Type, string Value)[] claims) =>
        new(new ClaimsIdentity(claims.Select(x => new Claim(x.Type, x.Value)), "test"));

    [Fact]
    public void ReadsObjectIdNameAndUsernameFromTheToken()
    {
        // Arrange
        var principal = Principal(("oid", "oid-1"), ("name", "Anna Svensson"), ("preferred_username", "anna@example.com"));

        // Act
        var identity = EntraIdentity.From(principal);

        // Assert
        identity.ShouldBe(new EntraIdentity("oid-1", "Anna Svensson", "anna@example.com"));
    }

    [Fact]
    public void AcceptsTheLongFormObjectIdClaim()
    {
        // Arrange
        var principal = Principal(("http://schemas.microsoft.com/identity/claims/objectidentifier", "oid-2"), ("upn", "bo@example.com"));

        // Act
        var identity = EntraIdentity.From(principal);

        // Assert
        identity.ShouldNotBeNull().ExternalId.ShouldBe("oid-2");
        identity.Username.ShouldBe("bo@example.com");
    }

    [Fact]
    public void FallsBackToTheUsernameWhenThereIsNoDisplayName()
    {
        // Arrange
        var principal = Principal(("oid", "oid-3"), ("preferred_username", "cia@example.com"));

        // Act
        var identity = EntraIdentity.From(principal);

        // Assert
        identity.ShouldNotBeNull().DisplayName.ShouldBe("cia@example.com");
    }

    [Fact]
    public void FallsBackToTheObjectIdWhenThereIsNoUsernameEither()
    {
        // Arrange
        var principal = Principal(("oid", "oid-4"));

        // Act
        var identity = EntraIdentity.From(principal);

        // Assert
        identity.ShouldNotBeNull().Username.ShouldBe("oid-4");
    }

    [Fact]
    public void ReturnsNothingForATokenWithoutAnObjectId()
    {
        // Arrange
        var principal = Principal(("name", "Local Admin"), ("user_id", Guid.NewGuid().ToString()));

        // Act
        var identity = EntraIdentity.From(principal);

        // Assert
        identity.ShouldBeNull();
    }
}
