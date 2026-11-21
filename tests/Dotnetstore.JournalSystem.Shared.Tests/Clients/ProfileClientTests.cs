using System.Net;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Shared.Tests.Clients;

public sealed class ProfileClientTests
{
    private const string Body =
        """{"id":"11111111-2222-3333-4444-555555555555","firstName":"Anna","middleName":null,"lastName":"Svensson","username":"anna@example.com","authProvider":1,"dateOfBirth":"1990-01-01","isMale":false,"socialSecurityNumberMasked":"19900101-****","ownCompanyId":"11111111-2222-3333-4444-555555555556","ownCompanyName":"Dotnetstore AB","roleId":"11111111-2222-3333-4444-555555555557","roleName":"Doctor","lastLoginAt":null}""";

    [Fact]
    public async Task GetMineReadsTheProfileFromTheMeRoute()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.OK, Body));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IProfileClient>();

        // Act
        var result = await client.GetMineAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var profile = result.Value.ShouldNotBeNull();
        profile.FullName.ShouldBe("Anna Svensson");
        profile.SocialSecurityNumberMasked.ShouldBe("19900101-****");
        handler.Requests.Single().Method.ShouldBe(HttpMethod.Get);
        handler.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/api/organization/users/me");
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task GetMineReportsFailuresWithoutAProfile(HttpStatusCode status)
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(status, "{}"));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IProfileClient>();

        // Act
        var result = await client.GetMineAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Value.ShouldBeNull();
    }
}
