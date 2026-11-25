namespace Dotnetstore.JournalSystem.Shared.Constants;

public static class OwnCompanyImageConstants
{
    public const int MaxSizeBytes = 2 * 1024 * 1024;

    // Room for the multipart envelope around a file of the maximum size.
    public const int MaxRequestBytes = MaxSizeBytes + (64 * 1024);

    public const int MaxPhotosPerCompany = 12;
    public const int FileNameMaxLength = 255;
    public const int ContentTypeMaxLength = 50;

    public const string FormFieldName = "file";
}
