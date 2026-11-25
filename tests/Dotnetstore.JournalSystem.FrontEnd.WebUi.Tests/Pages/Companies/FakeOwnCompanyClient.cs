using System.Net;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Pages.Companies;

internal sealed class FakeOwnCompanyClient : IOwnCompanyClient
{
    internal static OwnCompanyResponse Company(string name, bool isSystem = false) =>
        new(Guid.NewGuid(), name, null, null, null, null, null, isSystem);

    internal static ApiResult<OwnCompanyResponse> Saved(HttpStatusCode status) =>
        ApiResult.Create<OwnCompanyResponse>(status, status == HttpStatusCode.OK ? Company("Saved AB") : null);

    public Func<Task<ApiResult<IReadOnlyList<OwnCompanyResponse>>>> List { get; init; } =
        () => Task.FromResult(ApiResult.Create<IReadOnlyList<OwnCompanyResponse>>(HttpStatusCode.OK, []));

    public Func<ApiResult<OwnCompanyResponse>> Read { get; init; } = () => Saved(HttpStatusCode.OK);

    public Func<ApiResult<OwnCompanyResponse>> Write { get; init; } = () => Saved(HttpStatusCode.OK);

    public Func<ApiResult> Remove { get; init; } = () => ApiResult.Create(HttpStatusCode.NoContent);

    public Func<ApiResult<IReadOnlyList<OwnCompanyImageResponse>>> Images { get; init; } =
        () => ApiResult.Create<IReadOnlyList<OwnCompanyImageResponse>>(HttpStatusCode.OK, []);

    public Func<ApiResult<ApiFile>> Content { get; init; } =
        () => ApiResult.Create<ApiFile>(HttpStatusCode.OK, new ApiFile(new byte[] { 1, 2, 3 }, "image/png"));

    public Func<ApiResult<OwnCompanyImageResponse>> Upload { get; init; } =
        () => ApiResult.Create<OwnCompanyImageResponse>(HttpStatusCode.OK, null);

    public EditOwnCompanyRequest? Sent { get; private set; }

    public Guid? DeletedId { get; private set; }

    public Guid? DeletedImageId { get; private set; }

    public FileUpload? UploadedLogo { get; private set; }

    public FileUpload? UploadedPhoto { get; private set; }

    public Task<ApiResult<IReadOnlyList<OwnCompanyResponse>>> GetAllAsync(CancellationToken cancellationToken = default) => List();

    public Task<ApiResult<OwnCompanyResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Read());

    public Task<ApiResult<OwnCompanyResponse>> CreateAsync(EditOwnCompanyRequest request, CancellationToken cancellationToken = default)
    {
        Sent = request;
        return Task.FromResult(Write());
    }

    public Task<ApiResult<OwnCompanyResponse>> UpdateAsync(Guid id, EditOwnCompanyRequest request, CancellationToken cancellationToken = default)
    {
        Sent = request;
        return Task.FromResult(Write());
    }

    public Task<ApiResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeletedId = id;
        return Task.FromResult(Remove());
    }

    public Task<ApiResult<IReadOnlyList<OwnCompanyImageResponse>>> GetImagesAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Images());

    public Task<ApiResult<ApiFile>> GetImageContentAsync(Guid id, Guid imageId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Content());

    public Task<ApiResult<OwnCompanyImageResponse>> SetLogoAsync(Guid id, FileUpload file, CancellationToken cancellationToken = default)
    {
        UploadedLogo = file;
        return Task.FromResult(Upload());
    }

    public Task<ApiResult<OwnCompanyImageResponse>> AddPhotoAsync(Guid id, Guid imageId, FileUpload file, CancellationToken cancellationToken = default)
    {
        UploadedPhoto = file;
        return Task.FromResult(Upload());
    }

    public Task<ApiResult> DeleteImageAsync(Guid id, Guid imageId, CancellationToken cancellationToken = default)
    {
        DeletedImageId = imageId;
        return Task.FromResult(Remove());
    }
}
