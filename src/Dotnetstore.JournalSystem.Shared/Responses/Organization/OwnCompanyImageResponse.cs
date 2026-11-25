namespace Dotnetstore.JournalSystem.Shared.Responses.Organization;

public sealed record OwnCompanyImageResponse(
    Guid Id,
    bool IsLogo,
    string FileName,
    string ContentType,
    int SizeBytes,
    DateTime CreatedDate);
