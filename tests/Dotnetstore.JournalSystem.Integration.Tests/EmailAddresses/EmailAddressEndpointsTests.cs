using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.EmailAddresses;

public sealed class EmailAddressEndpointsTests(SqlServerFixture fixture)
{
    private const string BasePath = ApiEndpoints.Contact.EmailAddresses.GetAll;

    private static readonly Guid ObjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static Uri Url(string path = "") => new(BasePath + path, UriKind.Relative);

    private static EditEmailAddressRequest CreateRequest(Guid emailAddressTypeId, string address = "anna@example.com") =>
        new(emailAddressTypeId, ObjectId, address);

    private static async Task<Guid> CreateEmailAddressTypeAsync(HttpClient client)
    {
        var request = new EditEmailAddressTypeRequest("Work", "Work email");
        using var response = await client.PostAsJsonAsync(new Uri(ApiEndpoints.Contact.EmailAddressTypes.Create, UriKind.Relative), request, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<EmailAddressTypeResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull().Id;
    }

    private static async Task<EmailAddressResponse> CreateEmailAddressAsync(HttpClient client, Guid emailAddressTypeId, string address = "anna@example.com")
    {
        using var response = await client.PostAsJsonAsync(Url(), CreateRequest(emailAddressTypeId, address), TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<EmailAddressResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
    }

    private static async Task<JsonElement> ReadErrorsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("errors").Clone();
    }

    [Fact]
    public async Task GetAllReturnsEmptyListWhenNoEmailAddressesExist()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<EmailAddressResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllReturnsAllCreatedEmailAddresses()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreateEmailAddressTypeAsync(host.Client);
        await CreateEmailAddressAsync(host.Client, typeId, "a@example.com");
        await CreateEmailAddressAsync(host.Client, typeId, "b@example.com");

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<EmailAddressResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().Select(x => x.Address).ShouldBe(["a@example.com", "b@example.com"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByIdReturnsEmailAddress()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateEmailAddressAsync(host.Client, await CreateEmailAddressTypeAsync(host.Client));

        // Act
        using var response = await host.Client.GetAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);
        var emailAddress = await response.Content.ReadFromJsonAsync<EmailAddressResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        emailAddress.ShouldBe(created);
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
        var typeId = await CreateEmailAddressTypeAsync(host.Client);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(typeId), TestContext.Current.CancellationToken);
        var emailAddress = await response.Content.ReadFromJsonAsync<EmailAddressResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        emailAddress.ShouldNotBeNull();
        emailAddress.Id.ShouldNotBe(Guid.Empty);
        emailAddress.EmailAddressTypeId.ShouldBe(typeId);
        emailAddress.ObjectId.ShouldBe(ObjectId);
        emailAddress.Address.ShouldBe("anna@example.com");
        response.Headers.Location.ShouldNotBeNull().AbsolutePath.ShouldEndWith($"{BasePath}/{emailAddress.Id}");
    }

    [Fact]
    public async Task CreatedEmailAddressCanBeFetchedAtLocationHeader()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        using var created = await host.Client.PostAsJsonAsync(Url(), CreateRequest(await CreateEmailAddressTypeAsync(host.Client)), TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(created.Headers.Location, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("Anna <anna@example.com>")]
    public async Task CreateReturnsValidationProblemForMissingOrInvalidAddress(string address)
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(Guid.NewGuid(), address), TestContext.Current.CancellationToken);
        var errors = await ReadErrorsAsync(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        errors.TryGetProperty("Address", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateReturnsValidationProblemAndStoresNothingForTooLongAddress()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreateEmailAddressTypeAsync(host.Client);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(typeId, new string('a', DataSchemeConstants.EmailAddressMaxLength) + "@example.com"), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<EmailAddressResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateReturnsValidationProblemForEmptyReferenceIds()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var request = new EditEmailAddressRequest(Guid.Empty, Guid.Empty, "anna@example.com");

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), request, TestContext.Current.CancellationToken);
        var errors = await ReadErrorsAsync(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        errors.TryGetProperty("EmailAddressTypeId", out _).ShouldBeTrue();
        errors.TryGetProperty("ObjectId", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateReturnsValidationProblemAndStoresNothingForUnknownEmailAddressType()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(Guid.NewGuid()), TestContext.Current.CancellationToken);
        var errors = await ReadErrorsAsync(response);
        var items = await host.Client.GetFromJsonAsync<EmailAddressResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        errors.TryGetProperty("EmailAddressTypeId", out _).ShouldBeTrue();
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
    public async Task UpdateReturnsUpdatedEmailAddress()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreateEmailAddressTypeAsync(host.Client);
        var created = await CreateEmailAddressAsync(host.Client, typeId);

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(typeId, "changed@example.com"), TestContext.Current.CancellationToken);
        var updated = await response.Content.ReadFromJsonAsync<EmailAddressResponse>(TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<EmailAddressResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated.ShouldNotBeNull().Id.ShouldBe(created.Id);
        updated.Address.ShouldBe("changed@example.com");
        fetched.ShouldBe(updated);
    }

    [Fact]
    public async Task UpdateReturnsNotFoundForUnknownId()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreateEmailAddressTypeAsync(host.Client);

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{Guid.NewGuid()}"), CreateRequest(typeId), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateReturnsValidationProblemAndKeepsStoredValuesForInvalidRequest()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreateEmailAddressTypeAsync(host.Client);
        var created = await CreateEmailAddressAsync(host.Client, typeId, "original@example.com");

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(typeId, "not-an-email"), TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<EmailAddressResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        fetched.ShouldNotBeNull().Address.ShouldBe("original@example.com");
    }

    [Fact]
    public async Task UpdateReturnsValidationProblemAndKeepsStoredValuesForUnknownEmailAddressType()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateEmailAddressAsync(host.Client, await CreateEmailAddressTypeAsync(host.Client), "original@example.com");

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(Guid.NewGuid(), "changed@example.com"), TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<EmailAddressResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        fetched.ShouldNotBeNull().Address.ShouldBe("original@example.com");
    }

    [Fact]
    public async Task DeleteReturnsNoContentAndRemovesEmailAddress()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateEmailAddressAsync(host.Client, await CreateEmailAddressTypeAsync(host.Client));

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
        await CreateEmailAddressAsync(host.Client, await CreateEmailAddressTypeAsync(host.Client));

        // Act
        using var response = await host.Client.DeleteAsync(Url($"/{Guid.NewGuid()}"), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<EmailAddressResponse[]>(Url(), TestContext.Current.CancellationToken);

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
        using var response = await host.Client.PostAsJsonAsync(Url($"/{Guid.NewGuid()}"), CreateRequest(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }
}
