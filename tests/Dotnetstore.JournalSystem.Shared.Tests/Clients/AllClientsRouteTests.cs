using System.Net;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Contact;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Shared.Tests.Clients;

public sealed class AllClientsRouteTests
{
    public static TheoryData<Type, string> Clients => new()
    {
        { typeof(IOwnCompanyClient), "/api/organization/own-companies" },
        { typeof(IRoleClient), "/api/organization/roles" },
        { typeof(IUserClient), "/api/organization/users" },
        { typeof(IWorkGroupClient), "/api/organization/work-groups" },
        { typeof(IAddressTypeClient), "/api/contact/address-types" },
        { typeof(IPhoneNumberTypeClient), "/api/contact/phone-number-types" },
        { typeof(IEmailAddressTypeClient), "/api/contact/email-address-types" },
        { typeof(IAddressClient), "/api/contact/addresses" },
        { typeof(IEmailAddressClient), "/api/contact/email-addresses" },
        { typeof(IPhoneNumberClient), "/api/contact/phone-numbers" }
    };

    [Theory]
    [MemberData(nameof(Clients))]
    public async Task EveryClientCallsItsOwnCollectionAndItemRoutes(Type clientType, string route)
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Empty(HttpStatusCode.NoContent));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService(clientType);
        var id = Guid.NewGuid();
        var ct = TestContext.Current.CancellationToken;
        dynamic crud = client;

        // Act
        await crud.GetAllAsync(ct);
        await crud.GetByIdAsync(id, ct);
        await crud.DeleteAsync(id, ct);

        // Assert
        handler.Requests.Select(x => (x.Method, x.RequestUri!.AbsolutePath)).ShouldBe(
        [
            (HttpMethod.Get, route),
            (HttpMethod.Get, $"{route}/{id}"),
            (HttpMethod.Delete, $"{route}/{id}")
        ]);
    }

    [Fact]
    public async Task LoginPostsToTheLoginRoute()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.OK,
            """{"accessToken":"t","expiresAt":"2030-01-01T00:00:00+00:00"}"""));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IAuthClient>();

        // Act
        var result = await client.LoginAsync(new LoginRequest("u", "p"), TestContext.Current.CancellationToken);

        // Assert
        result.Value.ShouldNotBeNull().AccessToken.ShouldBe("t");
        handler.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/api/auth/login");
        handler.Requests.Single().Method.ShouldBe(HttpMethod.Post);
    }

    [Fact]
    public async Task LoginReturnsUnauthorizedWithoutThrowing()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Empty(HttpStatusCode.Unauthorized));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IAuthClient>();

        // Act
        var result = await client.LoginAsync(new LoginRequest("u", "bad"), TestContext.Current.CancellationToken);

        // Assert
        result.IsUnauthorized.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }
}
