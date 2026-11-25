namespace Dotnetstore.JournalSystem.Shared.Clients;

/// <summary>A file to upload. The content is buffered so that a retried request can send it again.</summary>
public sealed record FileUpload(string FileName, ReadOnlyMemory<byte> Content);
