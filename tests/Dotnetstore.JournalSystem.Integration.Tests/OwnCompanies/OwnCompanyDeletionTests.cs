using System.Net;
using System.Net.Http.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.OwnCompanies;

public sealed class OwnCompanyDeletionTests(SqlServerFixture fixture)
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static Uri Url(string path) => new(path, UriKind.Relative);

    [Fact]
    public async Task DeletingACompanyRemovesItsContactDataAndLeavesOtherCompaniesAlone()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, Token);
        var types = await CreateTypesAsync(host);
        var deleted = await CreateCompanyAsync(host, "Leaving AB");
        var kept = await CreateCompanyAsync(host, "Staying AB");
        await CreateContactDataAsync(host, types, deleted);
        await CreateContactDataAsync(host, types, kept);

        // Act
        using var response = await host.Client.DeleteAsync(Url($"{ApiEndpoints.Organization.OwnCompanies.GetAll}/{deleted}"), Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await WaitUntilAsync(async () => await CountContactDataAsync(host, deleted) == 0);
        (await CountContactDataAsync(host, kept)).ShouldBe(3);
    }

    [Fact]
    public async Task ARefusedDeleteKeepsTheContactData()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, Token);
        var types = await CreateTypesAsync(host);
        var companies = await host.Client.GetFromJsonAsync<OwnCompanyResponse[]>(
            Url(ApiEndpoints.Organization.OwnCompanies.GetAll), Token);
        var withUsers = companies!.Single(x => x.Name == "Test AB").Id;
        await host.SignInAsNewNonAdminAsync(Token);
        await host.SignInAsAdminAsync(Token);
        await CreateContactDataAsync(host, types, withUsers);

        // Act
        using var response = await host.Client.DeleteAsync(Url($"{ApiEndpoints.Organization.OwnCompanies.GetAll}/{withUsers}"), Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        // Nothing is published for a refused delete; the pause gives a wrongly published event time to show.
        await Task.Delay(TimeSpan.FromSeconds(2), Token);
        (await CountContactDataAsync(host, withUsers)).ShouldBe(3);
    }

    private static async Task<(Guid Address, Guid EmailAddress, Guid PhoneNumber)> CreateTypesAsync(ApiTestHost host)
    {
        var address = await PostAsync<AddressTypeResponse>(
            host, ApiEndpoints.Contact.AddressTypes.Create, new EditAddressTypeRequest("Visiting", "Visiting"));
        var emailAddress = await PostAsync<EmailAddressTypeResponse>(
            host, ApiEndpoints.Contact.EmailAddressTypes.Create, new EditEmailAddressTypeRequest("Support", "Support"));
        var phoneNumber = await PostAsync<PhoneNumberTypeResponse>(
            host, ApiEndpoints.Contact.PhoneNumberTypes.Create, new EditPhoneNumberTypeRequest("Work", "Work"));

        return (address.Id, emailAddress.Id, phoneNumber.Id);
    }

    private static async Task<Guid> CreateCompanyAsync(ApiTestHost host, string name) =>
        (await PostAsync<OwnCompanyResponse>(
            host,
            ApiEndpoints.Organization.OwnCompanies.Create,
            new EditOwnCompanyRequest(name, null, null, null, null, null))).Id;

    private static async Task CreateContactDataAsync(
        ApiTestHost host,
        (Guid Address, Guid EmailAddress, Guid PhoneNumber) types,
        Guid companyId)
    {
        await PostAsync<AddressResponse>(
            host,
            ApiEndpoints.Contact.Addresses.Create,
            new EditAddressRequest(types.Address, companyId, null, "Main Street 1", null, "12345", "Stockholm", "Stockholm", "Sweden"));
        await PostAsync<EmailAddressResponse>(
            host, ApiEndpoints.Contact.EmailAddresses.Create, new EditEmailAddressRequest(types.EmailAddress, companyId, "info@example.com"));
        await PostAsync<PhoneNumberResponse>(
            host, ApiEndpoints.Contact.PhoneNumbers.Create, new EditPhoneNumberRequest(types.PhoneNumber, companyId, "+46 70 123 45 67"));
    }

    private static async Task<T> PostAsync<T>(ApiTestHost host, string path, object request)
    {
        using var response = await host.Client.PostAsJsonAsync(Url(path), request, Token);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<T>(Token))!;
    }

    private static async Task<int> CountContactDataAsync(ApiTestHost host, Guid companyId)
    {
        var addresses = await host.Client.GetFromJsonAsync<AddressResponse[]>(
            Url($"{ApiEndpoints.Contact.Addresses.GetAll}?objectId={companyId}"), Token);
        var emailAddresses = await host.Client.GetFromJsonAsync<EmailAddressResponse[]>(
            Url($"{ApiEndpoints.Contact.EmailAddresses.GetAll}?objectId={companyId}"), Token);
        var phoneNumbers = await host.Client.GetFromJsonAsync<PhoneNumberResponse[]>(
            Url($"{ApiEndpoints.Contact.PhoneNumbers.GetAll}?objectId={companyId}"), Token);

        return addresses!.Length + emailAddresses!.Length + phoneNumbers!.Length;
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition)
    {
        var deadline = TimeProvider.System.GetUtcNow() + TimeSpan.FromSeconds(60);
        while (!await condition())
        {
            TimeProvider.System.GetUtcNow().ShouldBeLessThan(deadline, "The contact data was never removed.");
            await Task.Delay(TimeSpan.FromMilliseconds(200), Token);
        }
    }
}
