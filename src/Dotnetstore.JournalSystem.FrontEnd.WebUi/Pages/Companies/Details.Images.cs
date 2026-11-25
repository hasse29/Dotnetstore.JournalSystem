using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Companies;

public sealed partial class DetailsModel
{
    public OwnCompanyImageResponse? Logo { get; private set; }

    public IReadOnlyList<OwnCompanyImageResponse> Photos { get; private set; } = [];

    public bool ImagesUnavailable { get; private set; }

    // The browser has no API token, so images are fetched here and passed on.
    public async Task<IActionResult> OnGetImageAsync(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        var result = await TryAsync(() => ownCompanies.GetImageContentAsync(id, imageId, cancellationToken))
            .ConfigureAwait(false);
        if (result is not { IsSuccess: true, Value: { } file })
        {
            return NotFound();
        }

        // An image id never gets new content, so the browser may keep it despite the no-store default of the pages.
        Response.Headers.CacheControl = "private, max-age=86400";
        Response.Headers.XContentTypeOptions = "nosniff";

        return File(file.Content.ToArray(), file.ContentType);
    }

    public async Task<IActionResult> OnPostLogoAsync(Guid id, IFormFile? file, CancellationToken cancellationToken)
    {
        var upload = await ReadUploadAsync(file, cancellationToken).ConfigureAwait(false);
        if (upload is null)
        {
            return RedirectToPage(new { id });
        }

        return await DecideAsync(
                id,
                async () => await ownCompanies.SetLogoAsync(id, upload, cancellationToken).ConfigureAwait(false),
                "Logo saved.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostPhotoAsync(Guid id, IFormFile? file, CancellationToken cancellationToken)
    {
        var upload = await ReadUploadAsync(file, cancellationToken).ConfigureAwait(false);
        if (upload is null)
        {
            return RedirectToPage(new { id });
        }

        // Chosen once, so that a retried request stores the photo only once.
        var imageId = Guid.CreateVersion7();

        return await DecideAsync(
                id,
                async () => await ownCompanies.AddPhotoAsync(id, imageId, upload, cancellationToken).ConfigureAwait(false),
                "Photo added.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostDeleteImageAsync(Guid id, Guid imageId, CancellationToken cancellationToken) =>
        await DecideAsync(
                id,
                () => ownCompanies.DeleteImageAsync(id, imageId, cancellationToken),
                "Image deleted.")
            .ConfigureAwait(false);

    private async Task LoadImagesAsync(Guid id, CancellationToken cancellationToken)
    {
        var images = await LoadListAsync(() => ownCompanies.GetImagesAsync(id, cancellationToken)).ConfigureAwait(false);
        if (images is null)
        {
            ImagesUnavailable = true;
            return;
        }

        Logo = images.FirstOrDefault(x => x.IsLogo);
        Photos = images.Where(x => !x.IsLogo).ToList();
    }

    private async Task<FileUpload?> ReadUploadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            ErrorMessage = "Choose an image to upload.";
            return null;
        }

        if (file.Length > OwnCompanyImageConstants.MaxSizeBytes)
        {
            ErrorMessage = $"The image must be at most {OwnCompanyImageConstants.MaxSizeBytes / (1024 * 1024)} MB.";
            return null;
        }

        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);

        return new FileUpload(file.FileName, buffer.ToArray());
    }
}
