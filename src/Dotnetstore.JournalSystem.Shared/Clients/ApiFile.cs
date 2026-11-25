namespace Dotnetstore.JournalSystem.Shared.Clients;

/// <summary>A file downloaded from the API.</summary>
public sealed record ApiFile(ReadOnlyMemory<byte> Content, string ContentType);
