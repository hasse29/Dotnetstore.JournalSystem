namespace Dotnetstore.JournalSystem.Shared.Requests.Organization;

public sealed record LoginRequest(
    string Username,
    string Password);
