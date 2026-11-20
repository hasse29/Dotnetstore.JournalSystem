using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Authentication;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.Security.Extensions;
using Dotnetstore.Security.Handlers;
using Dotnetstore.Security.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Authentication;

public sealed class AuthServiceTests : IDisposable
{
    private const string Password = "a-long-enough-password";

    private readonly OrganizationTestDatabase _database;
    private readonly ServiceProvider _provider;
    private readonly IHashHandler _hashHandler;
    private readonly MutableTimeProvider _time = new(new DateTimeOffset(2026, 5, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly AuthService _service;
    private readonly Guid _roleId = Guid.NewGuid();

    public AuthServiceTests(SqlServerFixture fixture)
    {
        _database = new OrganizationTestDatabase(fixture);
        _provider = new ServiceCollection()
            .AddSecurity()
            .AddTokens(options =>
            {
                options.Issuer = "test-issuer";
                options.Audience = "test-audience";
                options.Algorithm = SigningAlgorithm.HS256;
                options.SigningKeys["k1"] = "ZGV2LW9ubHktdGVzdC1zaWduaW5nLWtleS0zMi1ieXRlcyE=";
                options.CurrentSigningKeyId = "k1";
            })
            .BuildServiceProvider();
        _hashHandler = _provider.GetRequiredService<IHashHandler>();
        _service = new AuthService(
            _database.UnitOfWork,
            _hashHandler,
            _provider.GetRequiredService<ITokenHandler>(),
            _time,
            NullLogger<AuthService>.Instance);

        _database.Context.Roles.Add(RoleTestData.Create(_roleId, "Nurse"));
        _database.Context.SaveChanges();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _database.Dispose();
    }

    private async Task SeedUserAsync(
        string username = "anna@example.com",
        AuthProvider provider = AuthProvider.Local,
        bool approved = true)
    {
        var request = UserTestData.CreateRequest(username, provider, null, Guid.NewGuid(), _roleId);
        var user = Dotnetstore.JournalSystem.Api.Organization.Users.UserBuilder
            .Create()
            .WithCreateValues(request, _hashHandler.Hash(Password), null, null)
            .SetAccountIsApproved(approved)
            .Build();
        _database.Context.Users.Add(user);
        await _database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _database.Context.ChangeTracker.Clear();
    }

    private async Task<Dotnetstore.JournalSystem.Api.Organization.Users.User> ReadUserAsync()
    {
        _database.Context.ChangeTracker.Clear();
        return await _database.Context.Users.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
    }

    private ValueTask<Result<Dotnetstore.JournalSystem.Shared.Responses.Organization.LoginResponse>> LoginAsync(string username, string password)
    {
        IAuthService service = _service;
        return service.LoginAsync(new LoginRequest(username, password), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task LoginWithCorrectPasswordReturnsTokenWithRoleClaims()
    {
        // Arrange
        await SeedUserAsync();

        // Act
        var result = await LoginAsync("anna@example.com", Password);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(result.Value.AccessToken);
        jwt.Claims.ShouldContain(x => x.Type == AuthClaimTypes.Role && x.Value == "Nurse");
        jwt.Claims.ShouldContain(x => x.Type == AuthClaimTypes.UserId);
        result.Value.ExpiresAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task LoginRecordsLastLoginAndResetsFailureCount()
    {
        // Arrange
        await SeedUserAsync();
        (await LoginAsync("anna@example.com", "wrong-password-123")).IsSuccess.ShouldBeFalse();

        // Act
        var result = await LoginAsync("anna@example.com", Password);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var stored = await ReadUserAsync();
        stored.AccessFailedCount.ShouldBe(0);
        stored.LastLoginAt.ShouldBe(_time.GetUtcNow().UtcDateTime);
    }

    [Fact]
    public async Task LoginWithWrongPasswordIsUnauthorizedAndCountsTheFailure()
    {
        // Arrange
        await SeedUserAsync();

        // Act
        var result = await LoginAsync("anna@example.com", "wrong-password-123");

        // Assert
        result.Status.ShouldBe(ResultStatus.Unauthorized);
        (await ReadUserAsync()).AccessFailedCount.ShouldBe(1);
    }

    [Fact]
    public async Task LoginWithUnknownUserIsUnauthorized()
    {
        // Arrange & Act
        var result = await LoginAsync("nobody@example.com", Password);

        // Assert
        result.Status.ShouldBe(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task LoginForEntraUserIsUnauthorizedEvenWithMatchingHash()
    {
        // Arrange
        await SeedUserAsync(provider: AuthProvider.Entra);

        // Act
        var result = await LoginAsync("anna@example.com", Password);

        // Assert
        result.Status.ShouldBe(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task LoginForUnapprovedUserIsUnauthorized()
    {
        // Arrange
        await SeedUserAsync(approved: false);

        // Act
        var result = await LoginAsync("anna@example.com", Password);

        // Assert
        result.Status.ShouldBe(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task FifthFailureLocksTheAccountAndRejectsTheCorrectPassword()
    {
        // Arrange
        await SeedUserAsync();
        for (var i = 0; i < AuthService.MaxFailedAttempts; i++)
        {
            await LoginAsync("anna@example.com", "wrong-password-123");
        }

        // Act
        var result = await LoginAsync("anna@example.com", Password);

        // Assert
        result.Status.ShouldBe(ResultStatus.Unauthorized);
        var stored = await ReadUserAsync();
        stored.IsLockedOut.ShouldBeTrue();
        stored.LockoutEnd.ShouldBe(_time.GetUtcNow().UtcDateTime.Add(AuthService.LockoutDuration));
    }

    [Fact]
    public async Task FourFailuresDoNotLockTheAccount()
    {
        // Arrange
        await SeedUserAsync();
        for (var i = 0; i < AuthService.MaxFailedAttempts - 1; i++)
        {
            await LoginAsync("anna@example.com", "wrong-password-123");
        }

        // Act
        var result = await LoginAsync("anna@example.com", Password);

        // Assert
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task LockoutExpiresAfterTheLockoutDuration()
    {
        // Arrange
        await SeedUserAsync();
        for (var i = 0; i < AuthService.MaxFailedAttempts; i++)
        {
            await LoginAsync("anna@example.com", "wrong-password-123");
        }

        _time.Advance(AuthService.LockoutDuration + TimeSpan.FromSeconds(1));

        // Act
        var result = await LoginAsync("anna@example.com", Password);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var stored = await ReadUserAsync();
        stored.IsLockedOut.ShouldBeFalse();
        stored.AccessFailedCount.ShouldBe(0);
    }

    private sealed class MutableTimeProvider(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;

        public override DateTimeOffset GetUtcNow() => _now;

        internal void Advance(TimeSpan by) => _now = _now.Add(by);
    }
}
