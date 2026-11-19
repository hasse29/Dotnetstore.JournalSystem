using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.OwnCompanies;

public sealed class OwnCompanyEndpointsTests(SqlServerFixture fixture)
{
    private const string BasePath = ApiEndpoints.Organization.OwnCompanies.GetAll;

    private static Uri Url(string path = "") => new(BasePath + path, UriKind.Relative);

    private static EditOwnCompanyRequest CreateRequest(string name = "Dotnetstore AB") =>
        new(name, "Description", null, new Uri("https://example.com"), "556677-8899", "SE556677889901");

    private static async Task<OwnCompanyResponse> CreateCompanyAsync(HttpClient client, string name = "Dotnetstore AB")
    {
        using var response = await client.PostAsJsonAsync(Url(), CreateRequest(name), TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<OwnCompanyResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
    }

    private static async Task<JsonElement> ReadErrorsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("errors").Clone();
    }

    [Fact]
    public async Task GetAllReturnsEmptyListWhenNoCompaniesExist()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<OwnCompanyResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllReturnsAllCreatedCompanies()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        await CreateCompanyAsync(host.Client, "A");
        await CreateCompanyAsync(host.Client, "B");

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<OwnCompanyResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().Select(x => x.Name).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByIdReturnsCompany()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateCompanyAsync(host.Client);

        // Act
        using var response = await host.Client.GetAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);
        var company = await response.Content.ReadFromJsonAsync<OwnCompanyResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        company.ShouldBe(created);
    }

    [Fact]
    public async Task GetByIdReturnsNotFoundForUnknownId()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(Url($"/{Guid.NewGuid()}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByIdReturnsNotFoundForRouteThatIsNotAGuid()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(Url("/not-a-guid"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateReturnsCreatedWithLocationAndBody()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(), TestContext.Current.CancellationToken);
        var company = await response.Content.ReadFromJsonAsync<OwnCompanyResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        company.ShouldNotBeNull();
        company.Id.ShouldNotBe(Guid.Empty);
        company.Name.ShouldBe("Dotnetstore AB");
        company.TaxId.ShouldBe("SE556677889901");
        response.Headers.Location.ShouldNotBeNull().AbsolutePath.ShouldEndWith($"{BasePath}/{company.Id}");
    }

    [Fact]
    public async Task CreatedCompanyCanBeFetchedAtLocationHeader()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        using var created = await host.Client.PostAsJsonAsync(Url(), CreateRequest(), TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(created.Headers.Location, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateReturnsValidationProblemForMissingName(string name)
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(name), TestContext.Current.CancellationToken);
        var errors = await ReadErrorsAsync(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        errors.TryGetProperty("Name", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateReturnsValidationProblemAndStoresNothingForTooLongName()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(new string('x', 101)), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<OwnCompanyResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateReturnsBadRequestForEmptyBody()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        using var content = new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");

        // Act
        using var response = await host.Client.PostAsync(Url(), content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReturnsBadRequestForMalformedJson()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        using var content = new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json");

        // Act
        using var response = await host.Client.PostAsync(Url(), content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateReturnsUpdatedCompany()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateCompanyAsync(host.Client);

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest("Renamed AB"), TestContext.Current.CancellationToken);
        var updated = await response.Content.ReadFromJsonAsync<OwnCompanyResponse>(TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<OwnCompanyResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated.ShouldNotBeNull().Id.ShouldBe(created.Id);
        updated.Name.ShouldBe("Renamed AB");
        fetched.ShouldBe(updated);
    }

    [Fact]
    public async Task UpdateReturnsNotFoundForUnknownId()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{Guid.NewGuid()}"), CreateRequest(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateReturnsValidationProblemAndKeepsStoredValuesForInvalidRequest()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateCompanyAsync(host.Client, "Original AB");

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(""), TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<OwnCompanyResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        fetched.ShouldNotBeNull().Name.ShouldBe("Original AB");
    }

    [Fact]
    public async Task DeleteReturnsNoContentAndRemovesCompany()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateCompanyAsync(host.Client);

        // Act
        using var response = await host.Client.DeleteAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);
        using var afterwards = await host.Client.GetAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        afterwards.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteReturnsNotFoundAndKeepsDataForUnknownId()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        await CreateCompanyAsync(host.Client);

        // Act
        using var response = await host.Client.DeleteAsync(Url($"/{Guid.NewGuid()}"), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<OwnCompanyResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        items.ShouldNotBeNull().ShouldHaveSingleItem();
    }

    [Fact]
    public async Task UnsupportedMethodReturnsMethodNotAllowed()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url($"/{Guid.NewGuid()}"), CreateRequest(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }
}
