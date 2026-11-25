using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.WorkGroups;

public sealed class WorkGroupEndpointsTests(SqlServerFixture fixture)
{
    private const string BasePath = ApiEndpoints.Organization.WorkGroups.GetAll;

    private static Uri Url(string path = "") => new(BasePath + path, UriKind.Relative);

    private static EditWorkGroupRequest CreateRequest(string name = "Administrator") =>
        new(name, "Description");

    private static async Task<WorkGroupResponse> CreateWorkGroupAsync(HttpClient client, string name = "Administrator")
    {
        using var response = await client.PostAsJsonAsync(Url(), CreateRequest(name), TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<WorkGroupResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
    }

    private static async Task<JsonElement> ReadErrorsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("errors").Clone();
    }

    [Fact]
    public async Task GetAllReturnsEmptyListWhenNoWorkGroupsExist()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<WorkGroupResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllReturnsAllCreatedWorkGroups()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        await CreateWorkGroupAsync(host.Client, "A");
        await CreateWorkGroupAsync(host.Client, "B");

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<WorkGroupResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().Select(x => x.Name).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByIdReturnsWorkGroup()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateWorkGroupAsync(host.Client);

        // Act
        using var response = await host.Client.GetAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);
        var workGroup = await response.Content.ReadFromJsonAsync<WorkGroupResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        workGroup.ShouldBe(created);
    }

    [Fact]
    public async Task GetByIdReturnsNotFoundForUnknownId()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(Url($"/{Guid.NewGuid()}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByIdReturnsNotFoundForRouteThatIsNotAGuid()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(Url("/not-a-guid"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateReturnsCreatedWithLocationAndBody()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(), TestContext.Current.CancellationToken);
        var workGroup = await response.Content.ReadFromJsonAsync<WorkGroupResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        workGroup.ShouldNotBeNull();
        workGroup.Id.ShouldNotBe(Guid.Empty);
        workGroup.Name.ShouldBe("Administrator");
        workGroup.Description.ShouldBe("Description");
        response.Headers.Location.ShouldNotBeNull().AbsolutePath.ShouldEndWith($"{BasePath}/{workGroup.Id}");
    }

    [Fact]
    public async Task CreatedWorkGroupCanBeFetchedAtLocationHeader()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
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
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

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
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(new string('x', 101)), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<WorkGroupResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateReturnsBadRequestForEmptyBody()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
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
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        using var content = new StringContent("{ not json", System.Text.Encoding.UTF8, "application/json");

        // Act
        using var response = await host.Client.PostAsync(Url(), content, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateReturnsUpdatedWorkGroup()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateWorkGroupAsync(host.Client);

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest("Renamed"), TestContext.Current.CancellationToken);
        var updated = await response.Content.ReadFromJsonAsync<WorkGroupResponse>(TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<WorkGroupResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated.ShouldNotBeNull().Id.ShouldBe(created.Id);
        updated.Name.ShouldBe("Renamed");
        fetched.ShouldBe(updated);
    }

    [Fact]
    public async Task UpdateReturnsNotFoundForUnknownId()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{Guid.NewGuid()}"), CreateRequest(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateReturnsValidationProblemAndKeepsStoredValuesForInvalidRequest()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateWorkGroupAsync(host.Client, "Original");

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(""), TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<WorkGroupResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        fetched.ShouldNotBeNull().Name.ShouldBe("Original");
    }

    [Fact]
    public async Task DeleteReturnsNoContentAndRemovesWorkGroup()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateWorkGroupAsync(host.Client);

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
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        await CreateWorkGroupAsync(host.Client);

        // Act
        using var response = await host.Client.DeleteAsync(Url($"/{Guid.NewGuid()}"), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<WorkGroupResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        items.ShouldNotBeNull().ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData("GET", "")]
    [InlineData("GET", "/{id}")]
    [InlineData("POST", "")]
    [InlineData("PUT", "/{id}")]
    [InlineData("DELETE", "/{id}")]
    public async Task AnonymousCallersAreUnauthorized(string method, string path)
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        using var request = new HttpRequestMessage(new HttpMethod(method), Url(path.Replace("{id}", Guid.NewGuid().ToString(), StringComparison.Ordinal)));
        request.Content = JsonContent.Create(CreateRequest());

        // Act
        using var response = await host.Client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task NonAdministratorsCanReadWorkGroups()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateWorkGroupAsync(host.Client, "Readable");
        await host.SignInAsNewNonAdminAsync(TestContext.Current.CancellationToken);

        // Act
        using var all = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        using var one = await host.Client.GetAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        all.StatusCode.ShouldBe(HttpStatusCode.OK);
        one.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task NonAdministratorsCannotChangeWorkGroups()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateWorkGroupAsync(host.Client, "Original");
        await host.SignInAsNewNonAdminAsync(TestContext.Current.CancellationToken);

        // Act
        using var create = await host.Client.PostAsJsonAsync(Url(), CreateRequest("Sneaky"), TestContext.Current.CancellationToken);
        using var update = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest("Renamed"), TestContext.Current.CancellationToken);
        using var delete = await host.Client.DeleteAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<WorkGroupResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        create.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        update.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        delete.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var names = items.ShouldNotBeNull().Select(x => x.Name).ToArray();
        names.ShouldContain("Original");
        names.ShouldNotContain("Sneaky");
    }

    [Fact]
    public async Task UnsupportedMethodReturnsMethodNotAllowed()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url($"/{Guid.NewGuid()}"), CreateRequest(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }
}
