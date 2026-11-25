using System.Net;
using System.Net.Http.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Dotnetstore.JournalSystem.Shared.Clients;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.OwnCompanies;

public sealed class OwnCompanyImageEndpointsTests(SqlServerFixture fixture)
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static Uri Url(string template, Guid id, Guid? imageId = null) =>
        new(
            template
                .Replace("{id:guid}", id.ToString(), StringComparison.Ordinal)
                .Replace("{imageId:guid}", imageId?.ToString(), StringComparison.Ordinal),
            UriKind.Relative);

    private static byte[] Png(int length = 64)
    {
        var bytes = new byte[length];
        byte[] signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        signature.CopyTo(bytes, 0);
        return bytes;
    }

    private static async Task<HttpResponseMessage> UploadAsync(HttpClient client, Uri url, byte[] content)
    {
        using var form = new MultipartFormDataContent();
        using var file = new ByteArrayContent(content);
        form.Add(file, OwnCompanyImageConstants.FormFieldName, "picture.png");
        return await client.PutAsync(url, form, Token);
    }

    private async Task<(ApiTestHost Host, Guid CompanyId)> StartAsync()
    {
        var host = await ApiTestHost.CreateSignedInAsync(fixture, Token);
        var companies = await host.Client.GetFromJsonAsync<OwnCompanyResponse[]>(
            new Uri(ApiEndpoints.Organization.OwnCompanies.GetAll, UriKind.Relative),
            Token);
        return (host, companies![0].Id);
    }

    private static Task<OwnCompanyImageResponse[]?> ListAsync(HttpClient client, Guid companyId) =>
        client.GetFromJsonAsync<OwnCompanyImageResponse[]>(Url(ApiEndpoints.Organization.OwnCompanies.Images, companyId), Token);

    [Fact]
    public async Task UploadedLogoCanBeListedAndDownloaded()
    {
        // Arrange
        var (host, companyId) = await StartAsync();
        await using var _ = host;

        // Act
        using var upload = await UploadAsync(host.Client, Url(ApiEndpoints.Organization.OwnCompanies.Logo, companyId), Png());
        var logo = (await ListAsync(host.Client, companyId)).ShouldNotBeNull().ShouldHaveSingleItem();
        using var download = await host.Client.GetAsync(Url(ApiEndpoints.Organization.OwnCompanies.ImageById, companyId, logo.Id), Token);

        // Assert
        upload.StatusCode.ShouldBe(HttpStatusCode.OK);
        logo.IsLogo.ShouldBeTrue();
        logo.ContentType.ShouldBe("image/png");
        download.Content.Headers.ContentType!.MediaType.ShouldBe("image/png");
        (await download.Content.ReadAsByteArrayAsync(Token)).ShouldBe(Png());
    }

    [Fact]
    public async Task UploadingASecondLogoReplacesTheFirst()
    {
        // Arrange
        var (host, companyId) = await StartAsync();
        await using var _ = host;
        var url = Url(ApiEndpoints.Organization.OwnCompanies.Logo, companyId);
        (await UploadAsync(host.Client, url, Png())).Dispose();
        var first = (await ListAsync(host.Client, companyId))!.Single();

        // Act
        using var response = await UploadAsync(host.Client, url, Png(128));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var second = (await ListAsync(host.Client, companyId)).ShouldNotBeNull().ShouldHaveSingleItem();
        second.Id.ShouldNotBe(first.Id);
        second.SizeBytes.ShouldBe(128);
    }

    [Fact]
    public async Task RepeatingAPhotoUploadStoresItOnce()
    {
        // Arrange
        var (host, companyId) = await StartAsync();
        await using var _ = host;
        var url = Url(ApiEndpoints.Organization.OwnCompanies.Photo, companyId, Guid.NewGuid());

        // Act
        using var first = await UploadAsync(host.Client, url, Png());
        using var second = await UploadAsync(host.Client, url, Png());

        // Assert
        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        second.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ListAsync(host.Client, companyId)).ShouldNotBeNull().ShouldHaveSingleItem().IsLogo.ShouldBeFalse();
    }

    // The request size limit itself is enforced by Kestrel, which the in-memory test server does not run.
    [Fact]
    public async Task AFileOfTheMaximumSizeIsAccepted()
    {
        // Arrange
        var (host, companyId) = await StartAsync();
        await using var _ = host;

        // Act
        using var response = await UploadAsync(
            host.Client,
            Url(ApiEndpoints.Organization.OwnCompanies.Logo, companyId),
            Png(OwnCompanyImageConstants.MaxSizeBytes));

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TooLargeAndUnsupportedFilesAreRejected()
    {
        // Arrange
        var (host, companyId) = await StartAsync();
        await using var _ = host;
        var url = Url(ApiEndpoints.Organization.OwnCompanies.Logo, companyId);

        // Act
        using var tooLarge = await UploadAsync(host.Client, url, Png(OwnCompanyImageConstants.MaxSizeBytes + 1));
        using var svg = await UploadAsync(host.Client, url, "<svg xmlns=\"http://www.w3.org/2000/svg\"/>"u8.ToArray());

        // Assert
        tooLarge.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        svg.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ListAsync(host.Client, companyId)).ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task UploadingToAnUnknownCompanyReturnsNotFound()
    {
        // Arrange
        var (host, _) = await StartAsync();
        await using var __ = host;

        // Act
        using var response = await UploadAsync(host.Client, Url(ApiEndpoints.Organization.OwnCompanies.Logo, Guid.NewGuid()), Png());

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletedImagesAreGone()
    {
        // Arrange
        var (host, companyId) = await StartAsync();
        await using var _ = host;
        var imageId = Guid.NewGuid();
        (await UploadAsync(host.Client, Url(ApiEndpoints.Organization.OwnCompanies.Photo, companyId, imageId), Png())).Dispose();
        var url = Url(ApiEndpoints.Organization.OwnCompanies.ImageById, companyId, imageId);

        // Act
        using var deleted = await host.Client.DeleteAsync(url, Token);
        using var download = await host.Client.GetAsync(url, Token);
        using var again = await host.Client.DeleteAsync(url, Token);

        // Assert
        deleted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        download.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        again.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AnonymousCallersCannotReadOrUploadImages()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token);
        var companyId = Guid.NewGuid();

        // Act
        using var read = await host.Client.GetAsync(Url(ApiEndpoints.Organization.OwnCompanies.Images, companyId), Token);
        using var write = await UploadAsync(host.Client, Url(ApiEndpoints.Organization.OwnCompanies.Logo, companyId), Png());

        // Assert
        read.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        write.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task NonAdministratorsCanReadButNotUploadImages()
    {
        // Arrange
        var (host, companyId) = await StartAsync();
        await using var _ = host;
        await host.SignInAsNewNonAdminAsync(Token);

        // Act
        using var read = await host.Client.GetAsync(Url(ApiEndpoints.Organization.OwnCompanies.Images, companyId), Token);
        using var write = await UploadAsync(host.Client, Url(ApiEndpoints.Organization.OwnCompanies.Logo, companyId), Png());

        // Assert
        read.StatusCode.ShouldBe(HttpStatusCode.OK);
        write.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TheTypedClientUploadsListsDownloadsAndDeletes()
    {
        // Arrange
        var (host, companyId) = await StartAsync();
        await using var _ = host;
        var token = host.Client.DefaultRequestHeaders.Authorization!.Parameter!;
        await using var provider = host.CreateApiClients(new FixedTokenProvider(token));
        var client = provider.GetRequiredService<IOwnCompanyClient>();
        var imageId = Guid.NewGuid();

        // Act
        var added = await client.AddPhotoAsync(companyId, imageId, new FileUpload("picture.png", Png()), Token);
        var listed = await client.GetImagesAsync(companyId, Token);
        var file = await client.GetImageContentAsync(companyId, imageId, Token);
        var deleted = await client.DeleteImageAsync(companyId, imageId, Token);

        // Assert
        added.IsSuccess.ShouldBeTrue();
        listed.Value.ShouldNotBeNull().ShouldHaveSingleItem().FileName.ShouldBe("picture.png");
        file.Value.ShouldNotBeNull().Content.ToArray().ShouldBe(Png());
        deleted.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ACompanyWithImagesCanBeDeleted()
    {
        // Arrange
        var (host, _) = await StartAsync();
        await using var __ = host;
        using var created = await host.Client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Organization.OwnCompanies.Create, UriKind.Relative),
            new Shared.Requests.Organization.EditOwnCompanyRequest("Pictured AB", null, null, null, null, null),
            Token);
        var company = (await created.Content.ReadFromJsonAsync<OwnCompanyResponse>(Token))!;
        (await UploadAsync(host.Client, Url(ApiEndpoints.Organization.OwnCompanies.Logo, company.Id), Png())).Dispose();

        // Act
        using var response = await host.Client.DeleteAsync(Url(ApiEndpoints.Organization.OwnCompanies.Delete, company.Id), Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetByIdNamesWhoCreatedAndUpdatedTheCompany()
    {
        // Arrange
        var (host, _) = await StartAsync();
        await using var __ = host;
        var request = new Shared.Requests.Organization.EditOwnCompanyRequest("Audited AB", null, null, null, null, null);
        using var created = await host.Client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Organization.OwnCompanies.Create, UriKind.Relative), request, Token);
        var company = (await created.Content.ReadFromJsonAsync<OwnCompanyResponse>(Token))!;
        (await host.Client.PutAsJsonAsync(Url(ApiEndpoints.Organization.OwnCompanies.Update, company.Id), request, Token)).Dispose();

        // Act
        var stored = await host.Client.GetFromJsonAsync<OwnCompanyResponse>(
            Url(ApiEndpoints.Organization.OwnCompanies.GetById, company.Id), Token);

        // Assert
        stored.ShouldNotBeNull().CreatedByName.ShouldBe("System Administrator");
        stored.LastUpdatedByName.ShouldBe("System Administrator");
        stored.CreatedDate.ShouldNotBeNull();
        stored.LastUpdatedDate.ShouldNotBeNull();
    }

    private sealed class FixedTokenProvider(string token) : IAccessTokenProvider
    {
        public ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) => ValueTask.FromResult<string?>(token);
    }
}
