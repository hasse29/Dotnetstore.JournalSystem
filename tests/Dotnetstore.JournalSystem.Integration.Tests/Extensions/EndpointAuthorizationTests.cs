using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Extensions;

public sealed class EndpointAuthorizationTests(SqlServerFixture fixture)
{
    // Every route of the API and the one thing it takes to use it. A new route fails the test until it is added here.
    private static readonly (string Method, string Route, string Policy)[] Required =
    [
        ("GET", ApiEndpoints.Organization.OwnCompanies.GetAll, Permissions.OwnCompanies.View),
        ("GET", ApiEndpoints.Organization.OwnCompanies.GetById, Permissions.OwnCompanies.View),
        ("POST", ApiEndpoints.Organization.OwnCompanies.Create, Permissions.OwnCompanies.Create),
        ("PUT", ApiEndpoints.Organization.OwnCompanies.Update, Permissions.OwnCompanies.Edit),
        ("DELETE", ApiEndpoints.Organization.OwnCompanies.Delete, Permissions.OwnCompanies.Delete),
        ("GET", ApiEndpoints.Organization.OwnCompanies.Images, Permissions.OwnCompanies.View),
        ("GET", ApiEndpoints.Organization.OwnCompanies.ImageById, Permissions.OwnCompanies.View),
        ("PUT", ApiEndpoints.Organization.OwnCompanies.Logo, Permissions.OwnCompanies.Edit),
        ("PUT", ApiEndpoints.Organization.OwnCompanies.Photo, Permissions.OwnCompanies.Edit),
        ("DELETE", ApiEndpoints.Organization.OwnCompanies.ImageById, Permissions.OwnCompanies.Edit),
        ("GET", ApiEndpoints.Organization.Roles.GetAll, Permissions.Roles.View),
        ("GET", ApiEndpoints.Organization.Roles.GetById, Permissions.Roles.View),
        ("POST", ApiEndpoints.Organization.Roles.Create, AuthPolicies.Administrator),
        ("PUT", ApiEndpoints.Organization.Roles.Update, AuthPolicies.Administrator),
        ("DELETE", ApiEndpoints.Organization.Roles.Delete, AuthPolicies.Administrator),
        ("GET", ApiEndpoints.Organization.Users.GetAll, Permissions.Users.View),
        ("GET", ApiEndpoints.Organization.Users.GetById, Permissions.Users.View),
        ("POST", ApiEndpoints.Organization.Users.Create, Permissions.Users.Create),
        ("PUT", ApiEndpoints.Organization.Users.Update, Permissions.Users.Edit),
        ("DELETE", ApiEndpoints.Organization.Users.Delete, Permissions.Users.Delete),
        ("GET", ApiEndpoints.Organization.WorkGroups.GetAll, Permissions.WorkGroups.View),
        ("GET", ApiEndpoints.Organization.WorkGroups.GetById, Permissions.WorkGroups.View),
        ("POST", ApiEndpoints.Organization.WorkGroups.Create, Permissions.WorkGroups.Create),
        ("PUT", ApiEndpoints.Organization.WorkGroups.Update, Permissions.WorkGroups.Edit),
        ("DELETE", ApiEndpoints.Organization.WorkGroups.Delete, Permissions.WorkGroups.Delete),
        ("GET", ApiEndpoints.Organization.AccessRequests.GetAll, Permissions.AccessRequests.View),
        ("POST", ApiEndpoints.Organization.AccessRequests.Approve, Permissions.AccessRequests.Edit),
        ("POST", ApiEndpoints.Organization.AccessRequests.Reject, Permissions.AccessRequests.Edit),
        ("POST", ApiEndpoints.Organization.AccessRequests.Reopen, Permissions.AccessRequests.Edit),
        ("GET", ApiEndpoints.Organization.AccessRequests.Me, AuthPolicies.Authenticated),
        ("POST", ApiEndpoints.Organization.AccessRequests.Me, AuthPolicies.Authenticated),
        ("GET", ApiEndpoints.Organization.Users.Me, AuthPolicies.Authenticated),
        ("GET", ApiEndpoints.Contact.Addresses.GetAll, Permissions.ContactDetails.View),
        ("GET", ApiEndpoints.Contact.Addresses.GetById, Permissions.ContactDetails.View),
        ("POST", ApiEndpoints.Contact.Addresses.Create, Permissions.ContactDetails.Create),
        ("PUT", ApiEndpoints.Contact.Addresses.Update, Permissions.ContactDetails.Edit),
        ("DELETE", ApiEndpoints.Contact.Addresses.Delete, Permissions.ContactDetails.Delete),
        ("GET", ApiEndpoints.Contact.EmailAddresses.GetAll, Permissions.ContactDetails.View),
        ("GET", ApiEndpoints.Contact.EmailAddresses.GetById, Permissions.ContactDetails.View),
        ("POST", ApiEndpoints.Contact.EmailAddresses.Create, Permissions.ContactDetails.Create),
        ("PUT", ApiEndpoints.Contact.EmailAddresses.Update, Permissions.ContactDetails.Edit),
        ("DELETE", ApiEndpoints.Contact.EmailAddresses.Delete, Permissions.ContactDetails.Delete),
        ("GET", ApiEndpoints.Contact.PhoneNumbers.GetAll, Permissions.ContactDetails.View),
        ("GET", ApiEndpoints.Contact.PhoneNumbers.GetById, Permissions.ContactDetails.View),
        ("POST", ApiEndpoints.Contact.PhoneNumbers.Create, Permissions.ContactDetails.Create),
        ("PUT", ApiEndpoints.Contact.PhoneNumbers.Update, Permissions.ContactDetails.Edit),
        ("DELETE", ApiEndpoints.Contact.PhoneNumbers.Delete, Permissions.ContactDetails.Delete),
        ("GET", ApiEndpoints.Contact.AddressTypes.GetAll, Permissions.ContactTypes.View),
        ("GET", ApiEndpoints.Contact.AddressTypes.GetById, Permissions.ContactTypes.View),
        ("POST", ApiEndpoints.Contact.AddressTypes.Create, Permissions.ContactTypes.Create),
        ("PUT", ApiEndpoints.Contact.AddressTypes.Update, Permissions.ContactTypes.Edit),
        ("DELETE", ApiEndpoints.Contact.AddressTypes.Delete, Permissions.ContactTypes.Delete),
        ("GET", ApiEndpoints.Contact.EmailAddressTypes.GetAll, Permissions.ContactTypes.View),
        ("GET", ApiEndpoints.Contact.EmailAddressTypes.GetById, Permissions.ContactTypes.View),
        ("POST", ApiEndpoints.Contact.EmailAddressTypes.Create, Permissions.ContactTypes.Create),
        ("PUT", ApiEndpoints.Contact.EmailAddressTypes.Update, Permissions.ContactTypes.Edit),
        ("DELETE", ApiEndpoints.Contact.EmailAddressTypes.Delete, Permissions.ContactTypes.Delete),
        ("GET", ApiEndpoints.Contact.PhoneNumberTypes.GetAll, Permissions.ContactTypes.View),
        ("GET", ApiEndpoints.Contact.PhoneNumberTypes.GetById, Permissions.ContactTypes.View),
        ("POST", ApiEndpoints.Contact.PhoneNumberTypes.Create, Permissions.ContactTypes.Create),
        ("PUT", ApiEndpoints.Contact.PhoneNumberTypes.Update, Permissions.ContactTypes.Edit),
        ("DELETE", ApiEndpoints.Contact.PhoneNumberTypes.Delete, Permissions.ContactTypes.Delete)
    ];

    [Fact]
    public async Task EveryRouteButLoginNeedsExactlyTheRightListedForIt()
    {
        // Arrange
        var expected = Required.Select(x => $"{Normalize($"{x.Method} {x.Route}")} -> {x.Policy}").ToList();
        var login = Normalize("POST " + ApiEndpoints.Auth.Login);

        // Act
        var actual = (await ReadApiRoutesAsync())
            .Where(x => x.Route != login)
            .Select(x => $"{x.Route} -> {(x.Anonymous ? "anonymous" : string.Join(" + ", x.Policies))}")
            .ToList();

        // Assert
        actual.ShouldBe(expected, ignoreOrder: true);
    }

    [Fact]
    public async Task EveryRightIsNeededBySomeRoute()
    {
        // Act
        var policies = (await ReadApiRoutesAsync()).SelectMany(x => x.Policies).ToHashSet(StringComparer.Ordinal);

        // Assert
        Permissions.All.ShouldAllBe(x => policies.Contains(x));
    }

    private static string Normalize(string route) => route.Replace(" /", " ", StringComparison.Ordinal);

    private async Task<List<(string Route, string[] Policies, bool Anonymous)>> ReadApiRoutesAsync()
    {
        // Only routing metadata is read, but the host cannot start without its database.
        var connectionString = fixture.CreateDatabaseConnectionString();
        await TestDatabase.ProvisionAsync(connectionString, TestContext.Current.CancellationToken);
        await using var factory = TestHost.Create(
            "Production",
            new Dictionary<string, string?> { ["ConnectionStrings:journalsystem"] = connectionString });

        return factory.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .OfType<RouteEndpoint>()
            .Where(x => x.RoutePattern.RawText!.TrimStart('/').StartsWith("api/", StringComparison.Ordinal))
            .Select(x => (
                Route: Normalize($"{x.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods.Single()} {x.RoutePattern.RawText}"),
                Policies: x.Metadata.GetOrderedMetadata<IAuthorizeData>().Select(y => y.Policy!).ToArray(),
                Anonymous: x.Metadata.GetMetadata<IAllowAnonymous>() is not null))
            .ToList();
    }
}
