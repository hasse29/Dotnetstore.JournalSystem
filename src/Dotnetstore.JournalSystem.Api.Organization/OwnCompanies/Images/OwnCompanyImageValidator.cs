using Dotnetstore.JournalSystem.Shared.Constants;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;

internal static class OwnCompanyImageValidator
{
    internal static Dictionary<string, string[]> Validate(byte[]? content)
    {
        var errors = new Dictionary<string, string[]>();

        if (content is null || content.Length == 0)
        {
            errors[OwnCompanyImageConstants.FormFieldName] = ["A file is required."];
        }
        else if (content.Length > OwnCompanyImageConstants.MaxSizeBytes)
        {
            errors[OwnCompanyImageConstants.FormFieldName] = [$"The file must be at most {OwnCompanyImageConstants.MaxSizeBytes / (1024 * 1024)} MB."];
        }
        else if (DetectContentType(content) is null)
        {
            errors[OwnCompanyImageConstants.FormFieldName] = ["Only JPEG, PNG and WebP images are supported."];
        }

        return errors;
    }

    // The type is read from the file itself: the name and the content type sent by the caller cannot be trusted.
    internal static string? DetectContentType(ReadOnlySpan<byte> content)
    {
        if (content.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF]))
        {
            return "image/jpeg";
        }

        if (content.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
        {
            return "image/png";
        }

        if (content.Length >= 12 && content.StartsWith("RIFF"u8) && content[8..12].SequenceEqual("WEBP"u8))
        {
            return "image/webp";
        }

        return null;
    }
}
