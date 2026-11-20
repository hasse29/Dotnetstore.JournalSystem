using System.Net;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Dotnetstore.JournalSystem.Shared.Tests.Clients;

public sealed class BearerTokenAndRegistrationTests
{
    private sealed class FixedTokenProvider(string? token) : IAccessTokenProvider
    {
        public ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(token);
    }

    [Fact]
    public async Task TokenFromTheProviderIsSentAsABearerHeader()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.OK, "[]"));
        await using var provider = ClientsTestHost.Build(handler,
            s => s.AddSingleton<IAccessTokenProvider>(new FixedTokenProvider("abc")));
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        await client.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        var header = handler.Requests.Single().Headers.Authorization;
        header.ShouldNotBeNull();
        header.Scheme.ShouldBe("Bearer");
        header.Parameter.ShouldBe("abc");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task NoAuthorizationHeaderIsSentWithoutAToken(string? token)
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.OK, "[]"));
        await using var provider = ClientsTestHost.Build(handler,
            s => s.AddSingleton<IAccessTokenProvider>(new FixedTokenProvider(token)));
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        await client.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        handler.Requests.Single().Headers.Authorization.ShouldBeNull();
    }

    [Fact]
    public async Task DefaultProviderCallsTheApiAnonymously()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.OK, "[]"));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        await client.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        handler.Requests.Single().Headers.Authorization.ShouldBeNull();
    }

    [Fact]
    public async Task BaseAddressIsBoundFromConfiguration()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.OK, "[]"));
        await using var provider = ClientsTestHost.Build(handler, baseAddress: "https://other.test:7000");

        // Act
        var options = provider.GetRequiredService<IOptions<JournalApiClientOptions>>().Value;

        // Assert
        options.BaseAddress.ShouldBe(new Uri("https://other.test:7000"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("/relative")]
    public async Task MissingOrRelativeBaseAddressFailsValidation(string? baseAddress)
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Empty(HttpStatusCode.OK));
        await using var provider = ClientsTestHost.Build(handler, baseAddress: baseAddress);

        // Act
        var act = () => provider.GetRequiredService<IOptions<JournalApiClientOptions>>().Value;

        // Assert
        act.ShouldThrow<OptionsValidationException>();
    }
}
