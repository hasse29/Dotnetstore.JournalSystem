using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Shared.Responses.Organization;

public sealed record AccessRequestResponse(
    Guid Id,
    string DisplayName,
    string Username,
    AccessRequestStatus Status,
    DateTime RequestedAt,
    DateTime? DecidedAt,
    string? DecisionNote);
