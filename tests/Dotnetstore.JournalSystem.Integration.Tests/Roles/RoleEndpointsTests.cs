using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Roles;

public sealed class RoleEndpointsTests(SqlServerFixture fixture)
{
    private const string BasePath = ApiEndpoints.Organization.Roles.GetAll;

    private static Uri Url(string path = "") => new(BasePath + path, UriKind.Relative);

    private static EditRoleRequest CreateRequest(string name = "Doctor") =>
        new(name, "Description");

    private static async Task<RoleResponse> CreateRoleAsync(HttpClient client, string name = "Doctor")
    {
        using var response = await client.PostAsJsonAsync(Url(), CreateRequest(name), TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<RoleResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
    }

    private static async Task<JsonElement> ReadErrorsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("errors").Clone();
    }

    [Fact]
    public async Task GetAllReturnsOnlyTheAdministratorRoleWhenNoRolesAreCreated()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<RoleResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().ShouldHaveSingleItem().Name.ShouldBe(RoleNames.Administrator);
    }

    [Fact]
    public async Task GetAllReturnsAllCreatedRoles()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        await CreateRoleAsync(host.Client, "A");
        await CreateRoleAsync(host.Client, "B");

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<RoleResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().Select(x => x.Name).ShouldBe([RoleNames.Administrator, "A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByIdReturnsRole()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateRoleAsync(host.Client);

        // Act
        using var response = await host.Client.GetAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);
        var role = await response.Content.ReadFromJsonAsync<RoleResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        role.ShouldNotBeNull().ShouldBeEquivalentTo(created);
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
        var role = await response.Content.ReadFromJsonAsync<RoleResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        role.ShouldNotBeNull();
        role.Id.ShouldNotBe(Guid.Empty);
        role.Name.ShouldBe("Doctor");
        role.Description.ShouldBe("Description");
        response.Headers.Location.ShouldNotBeNull().AbsolutePath.ShouldEndWith($"{BasePath}/{role.Id}");
    }

    [Fact]
    public async Task CreatedRoleCanBeFetchedAtLocationHeader()
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
        var items = await host.Client.GetFromJsonAsync<RoleResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        items.ShouldNotBeNull().ShouldHaveSingleItem().Name.ShouldBe(RoleNames.Administrator);
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
    public async Task UpdateReturnsUpdatedRole()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateRoleAsync(host.Client);

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest("Renamed"), TestContext.Current.CancellationToken);
        var updated = await response.Content.ReadFromJsonAsync<RoleResponse>(TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<RoleResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated.ShouldNotBeNull().Id.ShouldBe(created.Id);
        updated.Name.ShouldBe("Renamed");
        fetched.ShouldNotBeNull().ShouldBeEquivalentTo(updated);
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
        var created = await CreateRoleAsync(host.Client, "Original");

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(""), TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<RoleResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        fetched.ShouldNotBeNull().Name.ShouldBe("Original");
    }

    [Fact]
    public async Task DeleteReturnsNoContentAndRemovesRole()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateRoleAsync(host.Client);

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
        await CreateRoleAsync(host.Client);

        // Act
        using var response = await host.Client.DeleteAsync(Url($"/{Guid.NewGuid()}"), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<RoleResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        items.ShouldNotBeNull().Select(x => x.Name).ShouldBe([RoleNames.Administrator, "Doctor"], ignoreOrder: true);
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
    public async Task NonAdministratorsCanReadRoles()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateRoleAsync(host.Client, "Readable");
        await host.SignInAsNewNonAdminAsync(TestContext.Current.CancellationToken);

        // Act
        using var all = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        using var one = await host.Client.GetAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        all.StatusCode.ShouldBe(HttpStatusCode.OK);
        one.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task NonAdministratorsCannotChangeRoles()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateRoleAsync(host.Client, "Original");
        await host.SignInAsNewNonAdminAsync(TestContext.Current.CancellationToken);

        // Act
        using var create = await host.Client.PostAsJsonAsync(Url(), CreateRequest("Sneaky"), TestContext.Current.CancellationToken);
        using var update = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest("Renamed"), TestContext.Current.CancellationToken);
        using var delete = await host.Client.DeleteAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<RoleResponse[]>(Url(), TestContext.Current.CancellationToken);

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

    [Fact]
    public async Task ATakenNameIsAConflictThatSaysWhy()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateRoleAsync(host.Client);

        // Act
        using var create = await host.Client.PostAsJsonAsync(Url(), CreateRequest(), TestContext.Current.CancellationToken);
        using var update = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(RoleNames.Administrator), TestContext.Current.CancellationToken);

        // Assert
        create.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        update.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await create.Content.ReadFromJsonAsync<string>(TestContext.Current.CancellationToken)).ShouldBe("A role with that name already exists.");
    }

    [Fact]
    public async Task TheAdministratorRoleCannotBeRenamedOrDeleted()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var administrator = (await host.Client.GetFromJsonAsync<RoleResponse[]>(Url(), TestContext.Current.CancellationToken))
            .ShouldNotBeNull().ShouldHaveSingleItem();

        // Act
        using var rename = await host.Client.PutAsJsonAsync(Url($"/{administrator.Id}"), CreateRequest("Boss"), TestContext.Current.CancellationToken);
        using var delete = await host.Client.DeleteAsync(Url($"/{administrator.Id}"), TestContext.Current.CancellationToken);
        var afterwards = await host.Client.GetFromJsonAsync<RoleResponse>(Url($"/{administrator.Id}"), TestContext.Current.CancellationToken);

        // Assert
        administrator.IsSystem.ShouldBeTrue();
        rename.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        delete.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        afterwards.ShouldNotBeNull().ShouldBeEquivalentTo(administrator);
    }

    [Fact]
    public async Task ARoleThatAUserHasCannotBeDeleted()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        await host.SignInAsNewNonAdminAsync(TestContext.Current.CancellationToken);
        await host.SignInAsAdminAsync(TestContext.Current.CancellationToken);
        var inUse = (await host.Client.GetFromJsonAsync<RoleResponse[]>(Url(), TestContext.Current.CancellationToken))
            .ShouldNotBeNull().Single(x => x.Name == "Nurse");

        // Act
        using var response = await host.Client.DeleteAsync(Url($"/{inUse.Id}"), TestContext.Current.CancellationToken);
        using var afterwards = await host.Client.GetAsync(Url($"/{inUse.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        afterwards.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
