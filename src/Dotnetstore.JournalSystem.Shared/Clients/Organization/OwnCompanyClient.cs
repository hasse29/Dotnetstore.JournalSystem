using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Shared.Clients.Organization;

public interface IOwnCompanyClient : ICrudClient<EditOwnCompanyRequest, EditOwnCompanyRequest, OwnCompanyResponse>
{
    Task<ApiResult<IReadOnlyList<OwnCompanyImageResponse>>> GetImagesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiResult<ApiFile>> GetImageContentAsync(Guid id, Guid imageId, CancellationToken cancellationToken = default);

    Task<ApiResult<OwnCompanyImageResponse>> SetLogoAsync(Guid id, FileUpload file, CancellationToken cancellationToken = default);

    Task<ApiResult<OwnCompanyImageResponse>> AddPhotoAsync(
        Guid id,
        Guid imageId,
        FileUpload file,
        CancellationToken cancellationToken = default);

    Task<ApiResult> DeleteImageAsync(Guid id, Guid imageId, CancellationToken cancellationToken = default);
}

internal sealed class OwnCompanyClient(HttpClient http)
    : CrudClientBase<EditOwnCompanyRequest, EditOwnCompanyRequest, OwnCompanyResponse>(
        http,
        ApiEndpoints.Organization.OwnCompanies.GetAll,
        ApiEndpoints.Organization.OwnCompanies.GetById,
        ApiEndpoints.Organization.OwnCompanies.Create,
        ApiEndpoints.Organization.OwnCompanies.Update,
        ApiEndpoints.Organization.OwnCompanies.Delete),
      IOwnCompanyClient
{
    public async Task<ApiResult<IReadOnlyList<OwnCompanyImageResponse>>> GetImagesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<List<OwnCompanyImageResponse>>(
                HttpMethod.Get,
                BuildUri(ApiEndpoints.Organization.OwnCompanies.Images, id),
                null,
                cancellationToken)
            .ConfigureAwait(false);

        return ApiResult.Create<IReadOnlyList<OwnCompanyImageResponse>>(result.StatusCode, result.Value, result.Problem);
    }

    public Task<ApiResult<ApiFile>> GetImageContentAsync(Guid id, Guid imageId, CancellationToken cancellationToken = default) =>
        DownloadAsync(BuildImageUri(ApiEndpoints.Organization.OwnCompanies.ImageById, id, imageId), cancellationToken);

    public Task<ApiResult<OwnCompanyImageResponse>> SetLogoAsync(
        Guid id,
        FileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return UploadAsync<OwnCompanyImageResponse>(
            HttpMethod.Put,
            BuildUri(ApiEndpoints.Organization.OwnCompanies.Logo, id),
            OwnCompanyImageConstants.FormFieldName,
            file,
            cancellationToken);
    }

    public Task<ApiResult<OwnCompanyImageResponse>> AddPhotoAsync(
        Guid id,
        Guid imageId,
        FileUpload file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        return UploadAsync<OwnCompanyImageResponse>(
            HttpMethod.Put,
            BuildImageUri(ApiEndpoints.Organization.OwnCompanies.Photo, id, imageId),
            OwnCompanyImageConstants.FormFieldName,
            file,
            cancellationToken);
    }

    public Task<ApiResult> DeleteImageAsync(Guid id, Guid imageId, CancellationToken cancellationToken = default) =>
        SendAsync(
            HttpMethod.Delete,
            BuildImageUri(ApiEndpoints.Organization.OwnCompanies.ImageById, id, imageId),
            null,
            cancellationToken);

    private static Uri BuildImageUri(string template, Guid id, Guid imageId) =>
        BuildUri(template.Replace("{imageId:guid}", imageId.ToString("D"), StringComparison.Ordinal), id);
}
