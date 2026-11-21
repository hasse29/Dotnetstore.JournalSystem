using System.Net;
using System.Security.Claims;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Authentication;

public sealed class AccessGateTests
{
    private static ApiResult<AccessRequestResponse> Result(HttpStatusCode status, AccessRequestStatus? requestStatus = null)
    {
        AccessRequestResponse? value = requestStatus is { } s
            ? new AccessRequestResponse(Guid.NewGuid(), "Anna", "anna@example.com", s, DateTime.UtcNow, null, null)
            : null;
        return ApiResult.Create(status, value);
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) => new(new ClaimsIdentity(claims, "test"));

    [Fact]
    public void ApprovedPeopleAreLetIn()
    {
        // Arrange
        var result = Result(HttpStatusCode.OK, AccessRequestStatus.Approved);

        // Act & Assert
        AccessGate.Decide(result).ShouldBe(AccessGateOutcome.Allow);
    }

    [Theory]
    [InlineData(AccessRequestStatus.Pending)]
    [InlineData(AccessRequestStatus.Rejected)]
    public void PendingAndRejectedPeopleAreSentToTheStatusPage(AccessRequestStatus status)
    {
        // Arrange
        var result = Result(HttpStatusCode.OK, status);

        // Act & Assert
        AccessGate.Decide(result).ShouldBe(AccessGateOutcome.GoToStatus);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public void AnythingUncertainFailsClosedToTheStatusPage(HttpStatusCode status)
    {
        // Arrange
        var result = Result(status);

        // Act & Assert
        AccessGate.Decide(result).ShouldBe(AccessGateOutcome.GoToStatus);
    }

    [Fact]
    public void ASuccessfulReplyWithoutABodyIsNotTreatedAsApproval()
    {
        // Arrange
        var result = Result(HttpStatusCode.OK);

        // Act & Assert
        AccessGate.Decide(result).ShouldBe(AccessGateOutcome.GoToStatus);
    }

    [Fact]
    public void ARejectedTokenSignsTheSessionOut()
    {
        // Arrange
        var result = Result(HttpStatusCode.Unauthorized);

        // Act & Assert
        AccessGate.Decide(result).ShouldBe(AccessGateOutcome.SignOut);
    }

    [Fact]
    public void ATokenWithoutAMicrosoftIdentityHasNothingToApprove()
    {
        // Arrange
        var result = Result(HttpStatusCode.Forbidden);

        // Act & Assert
        AccessGate.Decide(result).ShouldBe(AccessGateOutcome.Allow);
    }

    [Theory]
    [InlineData("/Account/Login", true)]
    [InlineData("/Account/Logout", true)]
    [InlineData("/Access/Status", true)]
    [InlineData("/Error", true)]
    [InlineData("/account/login", true)]
    [InlineData("/Dashboard/Index", false)]
    [InlineData("/Admin/AccessRequests", false)]
    [InlineData("/AccountSettings", true)]
    [InlineData(null, false)]
    public void OnlySignInAndStatusPagesSkipTheGate(string? path, bool exempt)
    {
        // Arrange & Act
        var result = AccessGate.IsExempt(path);

        // Assert
        result.ShouldBe(exempt);
    }

    [Fact]
    public void PasswordSessionsCarryTheApplicationsOwnUserId()
    {
        // Arrange
        var principal = Principal(new Claim(AuthClaimTypes.UserId, Guid.NewGuid().ToString()));

        // Act & Assert
        AccessGate.HasApplicationAccount(principal).ShouldBeTrue();
        AccessGate.HasApplicationAccount(Principal(new Claim("oid", "x"))).ShouldBeFalse();
    }

    [Fact]
    public void TheAdministrationLinkIsShownToAdministratorsAndToUnapprovedMicrosoftSessions()
    {
        // Arrange
        var admin = Principal(new Claim(AuthClaimTypes.UserId, "1"), new Claim(AuthClaimTypes.Role, RoleNames.Administrator));
        var microsoft = Principal(new Claim("oid", "x"));

        // Act & Assert
        AccessGate.IsAdministratorOrUnknown(admin).ShouldBeTrue();
        AccessGate.IsAdministratorOrUnknown(microsoft).ShouldBeTrue();
    }

    [Fact]
    public void TheAdministrationLinkIsHiddenFromOrdinaryPasswordUsers()
    {
        // Arrange
        var nurse = Principal(new Claim(AuthClaimTypes.UserId, "2"), new Claim(AuthClaimTypes.Role, "Nurse"));

        // Act & Assert
        AccessGate.IsAdministratorOrUnknown(nurse).ShouldBeFalse();
    }
}
