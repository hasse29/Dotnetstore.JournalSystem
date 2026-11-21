using Dotnetstore.JournalSystem.Api.Organization.AccessRequests;
using Dotnetstore.JournalSystem.Shared.Constants;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.AccessRequests;

public sealed class AccessRequestTests
{
    private static readonly DateTime Now = new(2026, 11, 21, 10, 0, 0, DateTimeKind.Utc);

    private static AccessRequest Pending() => AccessRequest.Create("oid-1", "Anna Svensson", "anna@example.com", Now);

    [Fact]
    public void CreateStartsPendingWithNoDecision()
    {
        // Arrange & Act
        var request = Pending();

        // Assert
        request.Status.ShouldBe(AccessRequestStatus.Pending);
        request.RequestedAt.ShouldBe(Now);
        request.DecidedAt.ShouldBeNull();
        request.DecidedBy.ShouldBeNull();
        request.DecisionNote.ShouldBeNull();
        request.Id.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("", "name", "user")]
    [InlineData("oid", " ", "user")]
    [InlineData("oid", "name", "")]
    public void CreateRejectsMissingIdentityValues(string externalId, string displayName, string username)
    {
        // Arrange & Act
        var act = () => AccessRequest.Create(externalId, displayName, username, Now);

        // Assert
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void CreateTruncatesOverlongValuesInsteadOfFailingTheSignIn()
    {
        // Arrange & Act
        var request = AccessRequest.Create("oid", new string('n', 500), new string('u', 500), Now);

        // Assert
        request.DisplayName.Length.ShouldBe(DataSchemeConstants.AccessRequestDisplayNameMaxLength);
        request.Username.Length.ShouldBe(DataSchemeConstants.UserUsernameMaxLength);
    }

    [Fact]
    public void ApproveMarksAPendingRequestApprovedAndRecordsWhoDecided()
    {
        // Arrange
        var request = Pending();
        var admin = Guid.NewGuid();

        // Act
        var approved = request.TryApprove(admin, Now.AddHours(1));

        // Assert
        approved.ShouldBeTrue();
        request.Status.ShouldBe(AccessRequestStatus.Approved);
        request.DecidedBy.ShouldBe(admin);
        request.DecidedAt.ShouldBe(Now.AddHours(1));
    }

    [Fact]
    public void ApproveDoesNothingWhenTheRequestIsNotPending()
    {
        // Arrange
        var request = Pending();
        request.TryReject(null, "no", Now);

        // Act
        var approved = request.TryApprove(Guid.NewGuid(), Now);

        // Assert
        approved.ShouldBeFalse();
        request.Status.ShouldBe(AccessRequestStatus.Rejected);
    }

    [Fact]
    public void RejectStoresATrimmedNote()
    {
        // Arrange
        var request = Pending();

        // Act
        var rejected = request.TryReject(Guid.NewGuid(), "  Not an employee  ", Now);

        // Assert
        rejected.ShouldBeTrue();
        request.Status.ShouldBe(AccessRequestStatus.Rejected);
        request.DecisionNote.ShouldBe("Not an employee");
    }

    [Fact]
    public void RejectWithABlankNoteStoresNoNote()
    {
        // Arrange
        var request = Pending();

        // Act
        request.TryReject(null, "   ", Now);

        // Assert
        request.DecisionNote.ShouldBeNull();
    }

    [Fact]
    public void RejectDoesNothingOnAnApprovedRequest()
    {
        // Arrange
        var request = Pending();
        request.TryApprove(null, Now);

        // Act
        var rejected = request.TryReject(null, "late", Now);

        // Assert
        rejected.ShouldBeFalse();
        request.Status.ShouldBe(AccessRequestStatus.Approved);
    }

    [Fact]
    public void ReopenReturnsARejectedRequestToPendingAndClearsTheDecision()
    {
        // Arrange
        var request = Pending();
        request.TryReject(Guid.NewGuid(), "no", Now);

        // Act
        var reopened = request.TryReopen();

        // Assert
        reopened.ShouldBeTrue();
        request.Status.ShouldBe(AccessRequestStatus.Pending);
        request.DecidedAt.ShouldBeNull();
        request.DecidedBy.ShouldBeNull();
        request.DecisionNote.ShouldBeNull();
    }

    [Fact]
    public void ReopenDoesNothingUnlessTheRequestIsRejected()
    {
        // Arrange
        var pending = Pending();
        var approved = Pending();
        approved.TryApprove(null, Now);

        // Act & Assert
        pending.TryReopen().ShouldBeFalse();
        approved.TryReopen().ShouldBeFalse();
        approved.Status.ShouldBe(AccessRequestStatus.Approved);
    }
}
