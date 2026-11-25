using System.Net;
using System.Net.Http.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Addresses;

public sealed class ContactAccessTests(SqlServerFixture fixture)
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static Uri Url(string path) => new(path, UriKind.Relative);

    public static TheoryData<string> Collections => new()
    {
        ApiEndpoints.Contact.AddressTypes.GetAll,
        ApiEndpoints.Contact.Addresses.GetAll,
        ApiEndpoints.Contact.EmailAddressTypes.GetAll,
        ApiEndpoints.Contact.EmailAddresses.GetAll,
        ApiEndpoints.Contact.PhoneNumberTypes.GetAll,
        ApiEndpoints.Contact.PhoneNumbers.GetAll
    };

    [Theory]
    [MemberData(nameof(Collections))]
    public async Task AnonymousCallersCannotReadOrChangeContactData(string collection)
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token);

        // Act
        using var read = await host.Client.GetAsync(Url(collection), Token);
        using var write = await host.Client.DeleteAsync(Url($"{collection}/{Guid.NewGuid()}"), Token);

        // Assert
        read.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        write.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(Collections))]
    public async Task NonAdministratorsCanReadButNotChangeContactData(string collection)
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, Token);
        await host.SignInAsNewNonAdminAsync(Token);

        // Act
        using var read = await host.Client.GetAsync(Url(collection), Token);
        using var write = await host.Client.DeleteAsync(Url($"{collection}/{Guid.NewGuid()}"), Token);

        // Assert
        read.StatusCode.ShouldBe(HttpStatusCode.OK);
        write.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ListsCanBeLimitedToOneObject()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, Token);
        using var type = await host.Client.PostAsJsonAsync(
            Url(ApiEndpoints.Contact.PhoneNumberTypes.Create), new EditPhoneNumberTypeRequest("Work", "Work"), Token);
        var typeId = (await type.Content.ReadFromJsonAsync<PhoneNumberTypeResponse>(Token))!.Id;
        var wanted = Guid.NewGuid();
        foreach (var (objectId, number) in new[] { (wanted, "+46 70 111 11 11"), (Guid.NewGuid(), "+46 70 222 22 22") })
        {
            using var created = await host.Client.PostAsJsonAsync(
                Url(ApiEndpoints.Contact.PhoneNumbers.Create), new EditPhoneNumberRequest(typeId, objectId, number), Token);
            created.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Act
        var filtered = await host.Client.GetFromJsonAsync<PhoneNumberResponse[]>(
            Url($"{ApiEndpoints.Contact.PhoneNumbers.GetAll}?objectId={wanted}"), Token);
        var unknown = await host.Client.GetFromJsonAsync<PhoneNumberResponse[]>(
            Url($"{ApiEndpoints.Contact.PhoneNumbers.GetAll}?objectId={Guid.NewGuid()}"), Token);

        // Assert
        filtered.ShouldNotBeNull().ShouldHaveSingleItem().Number.ShouldBe("+46 70 111 11 11");
        unknown.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task AddressListCanBeLimitedToOneObject()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, Token);
        using var type = await host.Client.PostAsJsonAsync(
            Url(ApiEndpoints.Contact.AddressTypes.Create), new EditAddressTypeRequest("Visiting", "Visiting"), Token);
        var typeId = (await type.Content.ReadFromJsonAsync<AddressTypeResponse>(Token))!.Id;
        var wanted = Guid.NewGuid();
        foreach (var (objectId, street) in new[] { (wanted, "Wanted Street 1"), (Guid.NewGuid(), "Other Street 2") })
        {
            using var created = await host.Client.PostAsJsonAsync(
                Url(ApiEndpoints.Contact.Addresses.Create),
                new EditAddressRequest(typeId, objectId, null, street, null, "12345", "Stockholm", "Stockholm", "Sweden"),
                Token);
            created.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Act
        var filtered = await host.Client.GetFromJsonAsync<AddressResponse[]>(
            Url($"{ApiEndpoints.Contact.Addresses.GetAll}?objectId={wanted}"), Token);
        var unknown = await host.Client.GetFromJsonAsync<AddressResponse[]>(
            Url($"{ApiEndpoints.Contact.Addresses.GetAll}?objectId={Guid.NewGuid()}"), Token);

        // Assert
        filtered.ShouldNotBeNull().ShouldHaveSingleItem().Street1.ShouldBe("Wanted Street 1");
        unknown.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task EmailAddressListCanBeLimitedToOneObject()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, Token);
        using var type = await host.Client.PostAsJsonAsync(
            Url(ApiEndpoints.Contact.EmailAddressTypes.Create), new EditEmailAddressTypeRequest("Support", "Support"), Token);
        var typeId = (await type.Content.ReadFromJsonAsync<EmailAddressTypeResponse>(Token))!.Id;
        var wanted = Guid.NewGuid();
        foreach (var (objectId, address) in new[] { (wanted, "wanted@example.com"), (Guid.NewGuid(), "other@example.com") })
        {
            using var created = await host.Client.PostAsJsonAsync(
                Url(ApiEndpoints.Contact.EmailAddresses.Create), new EditEmailAddressRequest(typeId, objectId, address), Token);
            created.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Act
        var filtered = await host.Client.GetFromJsonAsync<EmailAddressResponse[]>(
            Url($"{ApiEndpoints.Contact.EmailAddresses.GetAll}?objectId={wanted}"), Token);
        var unknown = await host.Client.GetFromJsonAsync<EmailAddressResponse[]>(
            Url($"{ApiEndpoints.Contact.EmailAddresses.GetAll}?objectId={Guid.NewGuid()}"), Token);

        // Assert
        filtered.ShouldNotBeNull().ShouldHaveSingleItem().Address.ShouldBe("wanted@example.com");
        unknown.ShouldNotBeNull().ShouldBeEmpty();
    }
}
