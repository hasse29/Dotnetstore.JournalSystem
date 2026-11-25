using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Addresses;

public sealed class AddressEndpointsTests(SqlServerFixture fixture)
{
    private const string BasePath = ApiEndpoints.Contact.Addresses.GetAll;

    private static readonly Guid ObjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static Uri Url(string path = "") => new(BasePath + path, UriKind.Relative);

    private static EditAddressRequest CreateRequest(Guid addressTypeId, string street1 = "Main Street 1") =>
        new(addressTypeId, ObjectId, "c/o Anna", street1, "Floor 2", "12345", "Stockholm", "Stockholm County", "Sweden");

    private static async Task<Guid> CreateAddressTypeAsync(HttpClient client)
    {
        var request = new EditAddressTypeRequest("Home", "Home address");
        using var response = await client.PostAsJsonAsync(new Uri(ApiEndpoints.Contact.AddressTypes.Create, UriKind.Relative), request, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<AddressTypeResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull().Id;
    }

    private static async Task<AddressResponse> CreateAddressAsync(HttpClient client, Guid addressTypeId, string street1 = "Main Street 1")
    {
        using var response = await client.PostAsJsonAsync(Url(), CreateRequest(addressTypeId, street1), TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<AddressResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
    }

    private static async Task<JsonElement> ReadErrorsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("errors").Clone();
    }

    [Fact]
    public async Task GetAllReturnsEmptyListWhenNoAddressesExist()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<AddressResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllReturnsAllCreatedAddresses()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreateAddressTypeAsync(host.Client);
        await CreateAddressAsync(host.Client, typeId, "A");
        await CreateAddressAsync(host.Client, typeId, "B");

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<AddressResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().Select(x => x.Street1).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByIdReturnsAddress()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateAddressAsync(host.Client, await CreateAddressTypeAsync(host.Client));

        // Act
        using var response = await host.Client.GetAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);
        var address = await response.Content.ReadFromJsonAsync<AddressResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        address.ShouldBe(created);
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
        var typeId = await CreateAddressTypeAsync(host.Client);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(typeId), TestContext.Current.CancellationToken);
        var address = await response.Content.ReadFromJsonAsync<AddressResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        address.ShouldNotBeNull();
        address.Id.ShouldNotBe(Guid.Empty);
        address.AddressTypeId.ShouldBe(typeId);
        address.ObjectId.ShouldBe(ObjectId);
        address.CO.ShouldBe("c/o Anna");
        address.Street1.ShouldBe("Main Street 1");
        address.Country.ShouldBe("Sweden");
        response.Headers.Location.ShouldNotBeNull().AbsolutePath.ShouldEndWith($"{BasePath}/{address.Id}");
    }

    [Fact]
    public async Task CreatedAddressCanBeFetchedAtLocationHeader()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        using var created = await host.Client.PostAsJsonAsync(Url(), CreateRequest(await CreateAddressTypeAsync(host.Client)), TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(created.Headers.Location, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateReturnsValidationProblemForMissingStreet1(string street1)
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(Guid.NewGuid(), street1), TestContext.Current.CancellationToken);
        var errors = await ReadErrorsAsync(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        errors.TryGetProperty("Street1", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateReturnsValidationProblemAndStoresNothingForTooLongStreet1()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreateAddressTypeAsync(host.Client);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(typeId, new string('x', DataSchemeConstants.AddressStreetMaxLength + 1)), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<AddressResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateReturnsValidationProblemForEmptyReferenceIds()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var request = new EditAddressRequest(Guid.Empty, Guid.Empty, null, "Main Street 1", null, "12345", "Stockholm", "Stockholm", "Sweden");

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), request, TestContext.Current.CancellationToken);
        var errors = await ReadErrorsAsync(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        errors.TryGetProperty("AddressTypeId", out _).ShouldBeTrue();
        errors.TryGetProperty("ObjectId", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateReturnsValidationProblemAndStoresNothingForUnknownAddressType()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(Guid.NewGuid()), TestContext.Current.CancellationToken);
        var errors = await ReadErrorsAsync(response);
        var items = await host.Client.GetFromJsonAsync<AddressResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        errors.TryGetProperty("AddressTypeId", out _).ShouldBeTrue();
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
    public async Task UpdateReturnsUpdatedAddress()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreateAddressTypeAsync(host.Client);
        var created = await CreateAddressAsync(host.Client, typeId);

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(typeId, "Renamed Street"), TestContext.Current.CancellationToken);
        var updated = await response.Content.ReadFromJsonAsync<AddressResponse>(TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<AddressResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated.ShouldNotBeNull().Id.ShouldBe(created.Id);
        updated.Street1.ShouldBe("Renamed Street");
        fetched.ShouldBe(updated);
    }

    [Fact]
    public async Task UpdateReturnsNotFoundForUnknownId()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreateAddressTypeAsync(host.Client);

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{Guid.NewGuid()}"), CreateRequest(typeId), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateReturnsValidationProblemAndKeepsStoredValuesForInvalidRequest()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreateAddressTypeAsync(host.Client);
        var created = await CreateAddressAsync(host.Client, typeId, "Original Street");

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(typeId, ""), TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<AddressResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        fetched.ShouldNotBeNull().Street1.ShouldBe("Original Street");
    }

    [Fact]
    public async Task UpdateReturnsValidationProblemAndKeepsStoredValuesForUnknownAddressType()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateAddressAsync(host.Client, await CreateAddressTypeAsync(host.Client), "Original Street");

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(Guid.NewGuid(), "Changed Street"), TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<AddressResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        fetched.ShouldNotBeNull().Street1.ShouldBe("Original Street");
    }

    [Fact]
    public async Task DeleteReturnsNoContentAndRemovesAddress()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreateAddressAsync(host.Client, await CreateAddressTypeAsync(host.Client));

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
        await CreateAddressAsync(host.Client, await CreateAddressTypeAsync(host.Client));

        // Act
        using var response = await host.Client.DeleteAsync(Url($"/{Guid.NewGuid()}"), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<AddressResponse[]>(Url(), TestContext.Current.CancellationToken);

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
        using var response = await host.Client.PostAsJsonAsync(Url($"/{Guid.NewGuid()}"), CreateRequest(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task CreateStoresTheTextFieldsEncryptedAndStillReturnsPlainText()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreateAddressTypeAsync(host.Client);

        // Act
        var created = await CreateAddressAsync(host.Client, typeId, "Secret Street 9");
        var street = await host.ReadStoredTextAsync("SELECT [Street1] FROM [Contact].[Address] WHERE [Id] = @id", created.Id);
        var city = await host.ReadStoredTextAsync("SELECT [City] FROM [Contact].[Address] WHERE [Id] = @id", created.Id);
        var fetched = await host.Client.GetFromJsonAsync<AddressResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        street.ShouldNotBeNull().ShouldStartWith("dse1.");
        street.ShouldNotContain("Secret");
        city.ShouldNotBeNull().ShouldStartWith("dse1.");
        fetched.ShouldNotBeNull().Street1.ShouldBe("Secret Street 9");
        fetched.City.ShouldBe("Stockholm");
    }
}
