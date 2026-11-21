using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Api.Organization.AccessRequests;

internal sealed class AccessRequest
{
    private AccessRequest()
    {
    }

    public Guid Id { get; private set; }

    public string ExternalId { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string Username { get; private set; } = string.Empty;

    public AccessRequestStatus Status { get; private set; }

    public DateTime RequestedAt { get; private set; }

    public DateTime? DecidedAt { get; private set; }

    public Guid? DecidedBy { get; private set; }

    public string? DecisionNote { get; private set; }

    internal static AccessRequest Create(string externalId, string displayName, string username, DateTime now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        return new AccessRequest
        {
            Id = Guid.CreateVersion7(),
            ExternalId = externalId,
            DisplayName = Truncate(displayName, DataSchemeConstants.AccessRequestDisplayNameMaxLength),
            Username = Truncate(username, DataSchemeConstants.UserUsernameMaxLength),
            Status = AccessRequestStatus.Pending,
            RequestedAt = now
        };
    }

    internal bool TryApprove(Guid? decidedBy, DateTime now)
    {
        if (Status != AccessRequestStatus.Pending)
        {
            return false;
        }

        Decide(AccessRequestStatus.Approved, decidedBy, null, now);
        return true;
    }

    internal bool TryReject(Guid? decidedBy, string? note, DateTime now)
    {
        if (Status != AccessRequestStatus.Pending)
        {
            return false;
        }

        var trimmed = string.IsNullOrWhiteSpace(note)
            ? null
            : Truncate(note.Trim(), DataSchemeConstants.AccessRequestDecisionNoteMaxLength);
        Decide(AccessRequestStatus.Rejected, decidedBy, trimmed, now);
        return true;
    }

    internal bool TryReopen()
    {
        if (Status != AccessRequestStatus.Rejected)
        {
            return false;
        }

        Status = AccessRequestStatus.Pending;
        DecidedAt = null;
        DecidedBy = null;
        DecisionNote = null;
        return true;
    }

    private void Decide(AccessRequestStatus status, Guid? decidedBy, string? note, DateTime now)
    {
        Status = status;
        DecidedAt = now;
        DecidedBy = decidedBy;
        DecisionNote = note;
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
