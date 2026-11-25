using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Extensions;

public sealed class EndpointAuthorizationTests(SqlServerFixture fixture)
{
    // What a person needs before an administrator has approved them.
    private static readonly string[] OpenToUnapproved =
    [
        "GET " + ApiEndpoints.Organization.AccessRequests.Me,
        "POST " + ApiEndpoints.Organization.AccessRequests.Me,
        "GET " + ApiEndpoints.Organization.Users.Me
    ];

    [Fact]
    public async Task OnlyTheOwnRequestAndProfileRoutesAreOpenToTheUnapproved()
    {
        // Arrange
        var endpoints = await ReadApiRoutesAsync();

        // Act
        var open = endpoints
            .Where(x => x.Policies.Contains(AuthPolicies.Authenticated))
            .Select(x => x.Route);

        // Assert
        open.ShouldBe(OpenToUnapproved.Select(Normalize), ignoreOrder: true);
    }

    [Fact]
    public async Task EveryOtherRouteButLoginNeedsAnApprovedAccountOrTheAdministratorRole()
    {
        // Arrange
        var endpoints = await ReadApiRoutesAsync();
        var exceptions = OpenToUnapproved.Append("POST " + ApiEndpoints.Auth.Login).Select(Normalize).ToHashSet();

        // Act
        var unprotected = endpoints
            .Where(x => !exceptions.Contains(x.Route))
            .Where(x => x.Anonymous || x.Policies is not [AuthPolicies.Approved or AuthPolicies.Administrator])
            .Select(x => x.Route);

        // Assert
        endpoints.Count.ShouldBeGreaterThan(exceptions.Count);
        unprotected.ShouldBeEmpty();
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
