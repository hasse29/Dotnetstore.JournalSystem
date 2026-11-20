namespace Dotnetstore.JournalSystem.Shared.Clients;

/// <summary>What the API said went wrong, parsed from a problem-details, validation or plain-text body.</summary>
/// <param name="Title">Short summary, when the API sent one.</param>
/// <param name="Detail">Human-readable explanation, for example the reason for a 409 Conflict.</param>
/// <param name="Errors">Validation messages per field name. Empty when the failure was not a validation failure.</param>
public sealed record ApiProblem(
    string? Title,
    string? Detail,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static ApiProblem Empty { get; } = new(null, null, new Dictionary<string, string[]>());
}
