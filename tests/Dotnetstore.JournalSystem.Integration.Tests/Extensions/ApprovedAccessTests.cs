using System.Net;
using System.Net.Http.Json;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Extensions;

public sealed class ApprovedAccessTests(SqlServerFixture fixture)
{
    private const string ObjectId = "5b0f7a52-1d7e-4a53-9c1f-2f9a3f0c9e11";

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private static Uri Url(string path) => new(path, UriKind.Relative);

    public static TheoryData<string> Collections => new()
    {
        ApiEndpoints.Contact.AddressTypes.GetAll,
        ApiEndpoints.Contact.Addresses.GetAll,
        ApiEndpoints.Contact.EmailAddressTypes.GetAll,
        ApiEndpoints.Contact.EmailAddresses.GetAll,
        ApiEndpoints.Contact.PhoneNumberTypes.GetAll,
        ApiEndpoints.Contact.PhoneNumbers.GetAll,
        ApiEndpoints.Organization.OwnCompanies.GetAll,
        ApiEndpoints.Organization.Roles.GetAll,
        ApiEndpoints.Organization.WorkGroups.GetAll
    };

    [Theory]
    [MemberData(nameof(Collections))]
    public async Task AMicrosoftPersonWhoIsNotApprovedCannotRead(string collection)
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token);
        host.SignInAsMicrosoftPerson(ObjectId);

        // Act
        using var response = await host.Client.GetAsync(Url(collection), Token);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApprovalGivesTheSameSignInReadAccess()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateSignedInAsync(fixture, Token);
        var company = (await host.Client.GetFromJsonAsync<OwnCompanyResponse[]>(
            Url(ApiEndpoints.Organization.OwnCompanies.GetAll), Token))!.Single();
        using var roleResponse = await host.Client.PostAsJsonAsync(
            Url(ApiEndpoints.Organization.Roles.Create), new EditRoleRequest("Nurse", "Nurse"), Token);
        var role = (await roleResponse.Content.ReadFromJsonAsync<RoleResponse>(Token))!;

        host.SignInAsMicrosoftPerson(ObjectId);
        using var requested = await host.Client.PostAsync(Url(ApiEndpoints.Organization.AccessRequests.Me), null, Token);
        var request = (await requested.Content.ReadFromJsonAsync<AccessRequestResponse>(Token))!;
        using var pending = await host.Client.GetAsync(Url(ApiEndpoints.Organization.Roles.GetAll), Token);

        await host.SignInAsAdminAsync(Token);
        using var approval = await host.Client.PostAsJsonAsync(
            Url(ApiEndpoints.Organization.AccessRequests.Approve.Replace("{id:guid}", request.Id.ToString("D"), StringComparison.Ordinal)),
            new ApproveAccessRequestRequest(company.Id, role.Id, "Maria", null, "Microsoft", false),
            Token);
        host.SignInAsMicrosoftPerson(ObjectId);

        // Act
        using var approved = await host.Client.GetAsync(Url(ApiEndpoints.Organization.Roles.GetAll), Token);

        // Assert
        requested.IsSuccessStatusCode.ShouldBeTrue();
        pending.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        approval.IsSuccessStatusCode.ShouldBeTrue();
        approved.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
