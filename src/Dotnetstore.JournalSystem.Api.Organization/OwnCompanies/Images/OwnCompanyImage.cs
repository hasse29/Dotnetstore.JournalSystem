namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;

internal sealed class OwnCompanyImage
{
    public Guid Id { get; init; }

    public Guid OwnCompanyId { get; init; }

    public bool IsLogo { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public int SizeBytes { get; init; }

    public required byte[] Content { get; init; }

    public DateTime CreatedDate { get; init; }

    public Guid? CreatedBy { get; init; }
}
