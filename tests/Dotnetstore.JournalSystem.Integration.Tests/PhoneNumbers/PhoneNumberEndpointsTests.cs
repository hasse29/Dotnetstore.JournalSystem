using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.PhoneNumbers;

public sealed class PhoneNumberEndpointsTests(SqlServerFixture fixture)
{
    private const string BasePath = ApiEndpoints.Contact.PhoneNumbers.GetAll;

    private static readonly Guid ObjectId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static Uri Url(string path = "") => new(BasePath + path, UriKind.Relative);

    private static EditPhoneNumberRequest CreateRequest(Guid phoneNumberTypeId, string number = "+46 70 123 45 67") =>
        new(phoneNumberTypeId, ObjectId, number);

    private static async Task<Guid> CreatePhoneNumberTypeAsync(HttpClient client)
    {
        var request = new EditPhoneNumberTypeRequest("Work", "Work phone");
        using var response = await client.PostAsJsonAsync(new Uri(ApiEndpoints.Contact.PhoneNumberTypes.Create, UriKind.Relative), request, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<PhoneNumberTypeResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull().Id;
    }

    private static async Task<PhoneNumberResponse> CreatePhoneNumberAsync(HttpClient client, Guid phoneNumberTypeId, string number = "+46 70 123 45 67")
    {
        using var response = await client.PostAsJsonAsync(Url(), CreateRequest(phoneNumberTypeId, number), TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<PhoneNumberResponse>(TestContext.Current.CancellationToken)).ShouldNotBeNull();
    }

    private static async Task<JsonElement> ReadErrorsAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(body);
        return json.RootElement.GetProperty("errors").Clone();
    }

    [Fact]
    public async Task GetAllReturnsEmptyListWhenNoPhoneNumbersExist()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<PhoneNumberResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllReturnsAllCreatedPhoneNumbers()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreatePhoneNumberTypeAsync(host.Client);
        await CreatePhoneNumberAsync(host.Client, typeId, "+46 70 000 00 01");
        await CreatePhoneNumberAsync(host.Client, typeId, "+46 70 000 00 02");

        // Act
        using var response = await host.Client.GetAsync(Url(), TestContext.Current.CancellationToken);
        var items = await response.Content.ReadFromJsonAsync<PhoneNumberResponse[]>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        items.ShouldNotBeNull().Select(x => x.Number).ShouldBe(["+46 70 000 00 01", "+46 70 000 00 02"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByIdReturnsPhoneNumber()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreatePhoneNumberAsync(host.Client, await CreatePhoneNumberTypeAsync(host.Client));

        // Act
        using var response = await host.Client.GetAsync(Url($"/{created.Id}"), TestContext.Current.CancellationToken);
        var phoneNumber = await response.Content.ReadFromJsonAsync<PhoneNumberResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        phoneNumber.ShouldBe(created);
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
        var typeId = await CreatePhoneNumberTypeAsync(host.Client);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(typeId), TestContext.Current.CancellationToken);
        var phoneNumber = await response.Content.ReadFromJsonAsync<PhoneNumberResponse>(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        phoneNumber.ShouldNotBeNull();
        phoneNumber.Id.ShouldNotBe(Guid.Empty);
        phoneNumber.PhoneNumberTypeId.ShouldBe(typeId);
        phoneNumber.ObjectId.ShouldBe(ObjectId);
        phoneNumber.Number.ShouldBe("+46 70 123 45 67");
        response.Headers.Location.ShouldNotBeNull().AbsolutePath.ShouldEndWith($"{BasePath}/{phoneNumber.Id}");
    }

    [Fact]
    public async Task CreatedPhoneNumberCanBeFetchedAtLocationHeader()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        using var created = await host.Client.PostAsJsonAsync(Url(), CreateRequest(await CreatePhoneNumberTypeAsync(host.Client)), TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.GetAsync(created.Headers.Location, TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-number")]
    [InlineData("Call me 0701234567")]
    public async Task CreateReturnsValidationProblemForMissingOrInvalidAddress(string number)
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(Guid.NewGuid(), number), TestContext.Current.CancellationToken);
        var errors = await ReadErrorsAsync(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        errors.TryGetProperty("Number", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateReturnsValidationProblemAndStoresNothingForTooLongNumber()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreatePhoneNumberTypeAsync(host.Client);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(typeId, new string('1', DataSchemeConstants.PhoneNumberMaxLength + 1)), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<PhoneNumberResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        items.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateReturnsValidationProblemForEmptyReferenceIds()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var request = new EditPhoneNumberRequest(Guid.Empty, Guid.Empty, "+46 70 123 45 67");

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), request, TestContext.Current.CancellationToken);
        var errors = await ReadErrorsAsync(response);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        errors.TryGetProperty("PhoneNumberTypeId", out _).ShouldBeTrue();
        errors.TryGetProperty("ObjectId", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateReturnsValidationProblemAndStoresNothingForUnknownPhoneNumberType()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);

        // Act
        using var response = await host.Client.PostAsJsonAsync(Url(), CreateRequest(Guid.NewGuid()), TestContext.Current.CancellationToken);
        var errors = await ReadErrorsAsync(response);
        var items = await host.Client.GetFromJsonAsync<PhoneNumberResponse[]>(Url(), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        errors.TryGetProperty("PhoneNumberTypeId", out _).ShouldBeTrue();
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
    public async Task UpdateReturnsUpdatedPhoneNumber()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreatePhoneNumberTypeAsync(host.Client);
        var created = await CreatePhoneNumberAsync(host.Client, typeId);

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(typeId, "+46 70 999 99 99"), TestContext.Current.CancellationToken);
        var updated = await response.Content.ReadFromJsonAsync<PhoneNumberResponse>(TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<PhoneNumberResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        updated.ShouldNotBeNull().Id.ShouldBe(created.Id);
        updated.Number.ShouldBe("+46 70 999 99 99");
        fetched.ShouldBe(updated);
    }

    [Fact]
    public async Task UpdateReturnsNotFoundForUnknownId()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreatePhoneNumberTypeAsync(host.Client);

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
        var typeId = await CreatePhoneNumberTypeAsync(host.Client);
        var created = await CreatePhoneNumberAsync(host.Client, typeId, "+46 70 444 44 44");

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(typeId, "not-a-number"), TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<PhoneNumberResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        fetched.ShouldNotBeNull().Number.ShouldBe("+46 70 444 44 44");
    }

    [Fact]
    public async Task UpdateReturnsValidationProblemAndKeepsStoredValuesForUnknownPhoneNumberType()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreatePhoneNumberAsync(host.Client, await CreatePhoneNumberTypeAsync(host.Client), "+46 70 444 44 44");

        // Act
        using var response = await host.Client.PutAsJsonAsync(Url($"/{created.Id}"), CreateRequest(Guid.NewGuid(), "+46 70 999 99 99"), TestContext.Current.CancellationToken);
        var fetched = await host.Client.GetFromJsonAsync<PhoneNumberResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        fetched.ShouldNotBeNull().Number.ShouldBe("+46 70 444 44 44");
    }

    [Fact]
    public async Task DeleteReturnsNoContentAndRemovesPhoneNumber()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var created = await CreatePhoneNumberAsync(host.Client, await CreatePhoneNumberTypeAsync(host.Client));

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
        await CreatePhoneNumberAsync(host.Client, await CreatePhoneNumberTypeAsync(host.Client));

        // Act
        using var response = await host.Client.DeleteAsync(Url($"/{Guid.NewGuid()}"), TestContext.Current.CancellationToken);
        var items = await host.Client.GetFromJsonAsync<PhoneNumberResponse[]>(Url(), TestContext.Current.CancellationToken);

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
    public async Task CreateStoresTheNumberEncryptedAndStillReturnsPlainText()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, TestContext.Current.CancellationToken);
        var typeId = await CreatePhoneNumberTypeAsync(host.Client);

        // Act
        var created = await CreatePhoneNumberAsync(host.Client, typeId, "+46 70 987 65 43");
        var stored = await host.ReadStoredTextAsync("SELECT [Number] FROM [Contact].[PhoneNumber] WHERE [Id] = @id", created.Id);
        var fetched = await host.Client.GetFromJsonAsync<PhoneNumberResponse>(Url($"/{created.Id}"), TestContext.Current.CancellationToken);

        // Assert
        stored.ShouldNotBeNull().ShouldStartWith("dse1.");
        stored.ShouldNotContain("987");
        fetched.ShouldNotBeNull().Number.ShouldBe("+46 70 987 65 43");
    }
}
