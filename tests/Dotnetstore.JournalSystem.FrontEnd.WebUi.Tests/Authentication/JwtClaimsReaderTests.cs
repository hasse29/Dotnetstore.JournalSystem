using System.Security.Claims;
using System.Text;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.AspNetCore.WebUtilities;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Authentication;

public sealed class JwtClaimsReaderTests
{
    private const string Scheme = "Cookies";

    private static string Token(string payloadJson) =>
        $"{WebEncoders.Base64UrlEncode("{}"u8.ToArray())}.{WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson))}.sig";

    [Fact]
    public void NameIsTheUsernameTheUserTypedIn()
    {
        // Arrange
        var token = Token("{}");

        // Act
        var principal = JwtClaimsReader.CreatePrincipal(token, "anna@example.com", Scheme);

        // Assert
        principal.Identity!.Name.ShouldBe("anna@example.com");
        principal.Identity.IsAuthenticated.ShouldBeTrue();
        principal.Identity.AuthenticationType.ShouldBe(Scheme);
    }

    [Fact]
    public void ReadsUserIdOwnCompanyIdAndASingleRole()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var companyId = Guid.NewGuid().ToString();
        var token = Token($$"""{"user_id":"{{userId}}","own_company_id":"{{companyId}}","role":"Administrator"}""");

        // Act
        var principal = JwtClaimsReader.CreatePrincipal(token, "a", Scheme);

        // Assert
        principal.FindFirstValue(AuthClaimTypes.UserId).ShouldBe(userId);
        principal.FindFirstValue(AuthClaimTypes.OwnCompanyId).ShouldBe(companyId);
        principal.IsInRole("Administrator").ShouldBeTrue();
    }

    [Fact]
    public void ReadsEveryRoleWhenTheClaimIsAnArray()
    {
        // Arrange
        var token = Token("""{"role":["Administrator","Nurse",42]}""");

        // Act
        var principal = JwtClaimsReader.CreatePrincipal(token, "a", Scheme);

        // Assert
        principal.FindAll(AuthClaimTypes.Role).Select(x => x.Value).ShouldBe(["Administrator", "Nurse"]);
    }

    [Fact]
    public void TokenWithoutRolesGivesNoRoleClaims()
    {
        // Arrange
        var token = Token("""{"user_id":"x"}""");

        // Act
        var principal = JwtClaimsReader.CreatePrincipal(token, "a", Scheme);

        // Assert
        principal.IsInRole("Administrator").ShouldBeFalse();
        principal.FindAll(AuthClaimTypes.Role).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    [InlineData("a.b")]
    [InlineData("a.!!!.c")]
    public void MalformedTokenStillGivesAnAuthenticatedPrincipalWithOnlyTheName(string token)
    {
        // Arrange & Act
        var principal = JwtClaimsReader.CreatePrincipal(token, "anna", Scheme);

        // Assert
        principal.Identity!.Name.ShouldBe("anna");
        principal.Claims.Count().ShouldBe(1);
    }

    [Fact]
    public void PayloadThatIsNotJsonIsIgnored()
    {
        // Arrange
        var token = $"h.{WebEncoders.Base64UrlEncode("plain text"u8.ToArray())}.s";

        // Act
        var principal = JwtClaimsReader.CreatePrincipal(token, "anna", Scheme);

        // Assert
        principal.Claims.Count().ShouldBe(1);
    }
}
