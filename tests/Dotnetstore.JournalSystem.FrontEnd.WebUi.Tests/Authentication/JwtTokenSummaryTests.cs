using System.Text;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Authentication;

public sealed class JwtTokenSummaryTests
{
    private static string Token(string payloadJson) =>
        $"e30.{WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson))}.sig";

    [Fact]
    public void ListsAudienceIssuerVersionAndScopes()
    {
        // Arrange
        var token = Token("""{"aud":"api://abc","iss":"https://login/x/v2.0","ver":"2.0","scp":"access_as_user","name":"Secret Person"}""");

        // Act
        var summary = JwtTokenSummary.Describe(token);

        // Assert
        summary.ShouldContain("aud=api://abc");
        summary.ShouldContain("ver=2.0");
        summary.ShouldContain("scp=access_as_user");
    }

    [Fact]
    public void NeverIncludesPersonalClaims()
    {
        // Arrange
        var token = Token("""{"aud":"a","name":"Secret Person","preferred_username":"me@example.com"}""");

        // Act
        var summary = JwtTokenSummary.Describe(token);

        // Assert
        summary.ShouldNotContain("Secret");
        summary.ShouldNotContain("example.com");
    }

    [Theory]
    [InlineData(null, "no access token")]
    [InlineData("", "no access token")]
    [InlineData("opaque", "access token is not a JWT")]
    [InlineData("a.!!!.c", "access token payload is unreadable")]
    public void ExplainsTokensItCannotRead(string? token, string expected)
    {
        // Arrange & Act
        var summary = JwtTokenSummary.Describe(token);

        // Assert
        summary.ShouldBe(expected);
    }

    [Fact]
    public void ExplainsAPayloadThatIsNotJson()
    {
        // Arrange
        var token = $"e30.{WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("not json"))}.sig";

        // Act
        var summary = JwtTokenSummary.Describe(token);

        // Assert
        summary.ShouldBe("access token payload is unreadable");
    }
}
