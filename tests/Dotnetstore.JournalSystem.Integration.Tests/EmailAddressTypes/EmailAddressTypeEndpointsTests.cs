using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.EmailAddressTypes;

public sealed class EmailAddressTypeEndpointsTests(SqlServerFixture fixture)
{
    private const string BasePath = ApiEndpoints.Contact.EmailAddressTypes.GetAll;

    private static Uri Url(string path = "") => new(BasePath + path, UriKind.Relative);

    private static EditEmailAddressTypeRequest CreateRequest(string name = "Administrator") =>
        new(name, "Description");

    private static async Task<EmailAddressTypeResponse> CreateEmailAddressTypeAsync(HttpClient client, string name = "Administrator")
    {
        using var response = await client.PostAsJsonAsync(Url(), CreateRequest(name), TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<EmailAddressTypeResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
    }

    private static async Task<JsonElement> ReadErrorsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("errors").Clone();
    }

    [Fact]
    public async Task GetAllReturnsEmptyListWhenNoEmailAddressTypesExist()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<EmailAddressTypeResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllReturnsAllCreatedEmailAddressTypes()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        await CreateEmailAddressTypeAsync(host.Client, "A");
        await CreateEmailAddressTypeAsync(host.Client, "B");

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<EmailAddressTypeResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().Select(x => x.Name).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByIdReturnsEmailAddressType()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateEmailAddressTypeAsync(host.Client);

        // Act
        using var response = await host.Client.GetAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);
        var emailAddressType = await response.Content.ReadFromJsonAsync<EmailAddressTypeResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        emailAddressType.ShouldBe(created);
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
        var emailAddressType = await response.Content.ReadFromJsonAsync<EmailAddressTypeResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        emailAddressType.ShouldNotBeNull();
        emailAddressType.Id.ShouldNotBe(Guid.Empty);
        emailAddressType.Name.ShouldBe("Administrator");
        emailAddressType.Description.ShouldBe("Description");
        response.Headers.Location.ShouldNotBeNull().AbsolutePath.ShouldEndWith($"{BasePath}/{emailAddressType.Id}");
    }

    [Fact]
    public async Task CreatedEmailAddressTypeCanBeFetchedAtLocationHeader()
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
        var items = await host.Client.GetFromJsonAsync<EmailAddressTypeResponse[]>(Url(), TestContext.Current.CancellationToken);

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
    public async Task UpdateReturnsUpdatedEmailAddressType()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateEmailAddressTypeAsync(host.Client);

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest("Renamed"), TestContext.Current.CancellationToken);
        var updated = await response.Content.ReadFromJsonAsync<EmailAddressTypeResponse>(TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<EmailAddressTypeResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

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
        var created = await CreateEmailAddressTypeAsync(host.Client, "Original");

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(""), TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<EmailAddressTypeResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        fetched.ShouldNotBeNull().Name.ShouldBe("Original");
    }

    [Fact]
    public async Task DeleteReturnsNoContentAndRemovesEmailAddressType()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateEmailAddressTypeAsync(host.Client);

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
        await CreateEmailAddressTypeAsync(host.Client);

        // Act
        using var response = await host.Client.DeleteAsync(Url($"/{Guid.NewGuid()}"), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<EmailAddressTypeResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        items.ShouldNotBeNull().ShouldHaveSingleItem();
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
    public async Task DeleteReturnsConflictAndKeepsTheTypeWhenAnEmailAddressUsesIt()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var type = await CreateEmailAddressTypeAsync(host.Client);
        using var emailAddress = await host.Client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Contact.EmailAddresses.Create, UriKind.Relative),
            new EditEmailAddressRequest(type.Id, Guid.NewGuid(), "anna@example.com"),
            TestContext.Current.CancellationToken);
        emailAddress.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act
        using var response = await host.Client.DeleteAsync(Url($"/{type.Id}"), TestContext.Current.CancellationToken);
        using var afterwards = await host.Client.GetAsync(Url($"/{type.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldContain("in use");
        afterwards.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteSucceedsOnceTheEmailAddressUsingTheTypeIsDeleted()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var type = await CreateEmailAddressTypeAsync(host.Client);
        using var created = await host.Client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Contact.EmailAddresses.Create, UriKind.Relative),
            new EditEmailAddressRequest(type.Id, Guid.NewGuid(), "anna@example.com"),
            TestContext.Current.CancellationToken);
        var emailAddress = (await created.Content.ReadFromJsonAsync<EmailAddressResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
        using var deleteEmailAddress = await host.Client.DeleteAsync(new Uri($"{ApiEndpoints.Contact.EmailAddresses.GetAll}/{emailAddress.Id}", UriKind.Relative), TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.DeleteAsync(Url($"/{type.Id}"), TestContext.Current.CancellationToken);

        // Assert
        deleteEmailAddress.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
