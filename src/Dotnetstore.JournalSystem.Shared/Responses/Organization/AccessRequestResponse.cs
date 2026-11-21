using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Shared.Responses.Organization;

/// <param name="SuggestedFirstName">Guessed from the Microsoft display name to prefill the approval form.</param>
/// <param name="SuggestedLastName">Guessed from the Microsoft display name to prefill the approval form.</param>
public sealed record AccessRequestResponse(
    Guid Id,
    string DisplayName,
    string Username,
    AccessRequestStatus Status,
    DateTime RequestedAt,
    DateTime? DecidedAt,
    string? DecisionNote,
    string? SuggestedFirstName = null,
    string? SuggestedLastName = null);
