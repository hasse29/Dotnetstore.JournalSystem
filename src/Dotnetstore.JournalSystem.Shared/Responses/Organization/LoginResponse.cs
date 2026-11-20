namespace Dotnetstore.JournalSystem.Shared.Responses.Organization;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt);
