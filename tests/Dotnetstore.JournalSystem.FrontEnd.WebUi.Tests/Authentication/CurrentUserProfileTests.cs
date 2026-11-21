using System.Net;
using System.Security.Claims;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.Extensions.Caching.Memory;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Authentication;

public sealed class CurrentUserProfileTests
{
    private static readonly MyProfileResponse Anna = new(
        Guid.NewGuid(), "Anna", null, "Svensson", "anna@example.com", AuthProvider.Entra, null, false, "19900101-****",
        Guid.NewGuid(), "Dotnetstore AB", Guid.NewGuid(), "Doctor", null);

    private sealed class FakeProfileClient(Func<Task<ApiResult<MyProfileResponse>>> reply) : IProfileClient
    {
        public int Calls { get; private set; }

        public Task<ApiResult<MyProfileResponse>> GetMineAsync(CancellationToken cancellationToken = default)
        {
            Calls++;
            return reply();
        }
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "test"));

    private static ClaimsPrincipal SignedIn(string userId = "user-1") =>
        Principal(new Claim(AuthClaimTypes.UserId, userId));

    [Fact]
    public async Task LoadsTheProfileFromTheApi()
    {
        // Arrange
        var client = new FakeProfileClient(() => Task.FromResult(ApiResult.Create(HttpStatusCode.OK, Anna)));
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var profiles = new CurrentUserProfile(client, cache);

        // Act
        var profile = await profiles.GetAsync(SignedIn(), TestContext.Current.CancellationToken);

        // Assert
        profile.ShouldBe(Anna);
    }

    [Fact]
    public async Task ServesARepeatCallFromTheCacheInsteadOfTheApi()
    {
        // Arrange
        var client = new FakeProfileClient(() => Task.FromResult(ApiResult.Create(HttpStatusCode.OK, Anna)));
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var profiles = new CurrentUserProfile(client, cache);
        var principal = SignedIn();

        // Act
        await profiles.GetAsync(principal, TestContext.Current.CancellationToken);
        await profiles.GetAsync(principal, TestContext.Current.CancellationToken);

        // Assert
        client.Calls.ShouldBe(1);
    }

    [Fact]
    public async Task KeepsEachPersonsProfileApart()
    {
        // Arrange
        var client = new FakeProfileClient(() => Task.FromResult(ApiResult.Create(HttpStatusCode.OK, Anna)));
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var profiles = new CurrentUserProfile(client, cache);

        // Act
        await profiles.GetAsync(SignedIn("user-1"), TestContext.Current.CancellationToken);
        await profiles.GetAsync(SignedIn("user-2"), TestContext.Current.CancellationToken);

        // Assert
        client.Calls.ShouldBe(2);
    }

    [Fact]
    public async Task ReturnsNothingWhenTheApiHasNoProfileAndDoesNotCacheTheMiss()
    {
        // Arrange
        var client = new FakeProfileClient(() => Task.FromResult(ApiResult.Create<MyProfileResponse>(HttpStatusCode.NotFound, null)));
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var profiles = new CurrentUserProfile(client, cache);
        var principal = SignedIn();

        // Act
        var first = await profiles.GetAsync(principal, TestContext.Current.CancellationToken);
        await profiles.GetAsync(principal, TestContext.Current.CancellationToken);

        // Assert
        first.ShouldBeNull();
        client.Calls.ShouldBe(2);
    }

    [Fact]
    public async Task ReturnsNothingWhenTheApiCannotBeReached()
    {
        // Arrange
        var client = new FakeProfileClient(() => throw new HttpRequestException("down"));
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var profiles = new CurrentUserProfile(client, cache);

        // Act
        var profile = await profiles.GetAsync(SignedIn(), TestContext.Current.CancellationToken);

        // Assert
        profile.ShouldBeNull();
    }

    [Fact]
    public async Task ReturnsNothingForAnAnonymousVisitorWithoutCallingTheApi()
    {
        // Arrange
        var client = new FakeProfileClient(() => Task.FromResult(ApiResult.Create(HttpStatusCode.OK, Anna)));
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var profiles = new CurrentUserProfile(client, cache);

        // Act
        var profile = await profiles.GetAsync(new ClaimsPrincipal(new ClaimsIdentity()), TestContext.Current.CancellationToken);

        // Assert
        profile.ShouldBeNull();
        client.Calls.ShouldBe(0);
    }

    [Fact]
    public void CacheKeyUsesTheMicrosoftObjectIdWhenThereIsNoApplicationUserId()
    {
        // Arrange
        var principal = Principal(new Claim("oid", "oid-1"));

        // Act
        var key = CurrentUserProfile.CacheKey(principal);

        // Assert
        key.ShouldBe("profile:oid-1");
    }
}
