using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class UserRepositoryTests : IDisposable
{
    private static readonly DateTime LockoutEnd = new(2026, 5, 1, 12, 15, 0, DateTimeKind.Utc);
    private static readonly DateTime LoginAt = new(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly OrganizationTestDatabase _database;
    private readonly IUserRepository _repository;

    public UserRepositoryTests(SqlServerFixture fixture)
    {
        _database = new OrganizationTestDatabase(fixture);
        _repository = new UserRepository(_database.Context);
    }

    public void Dispose() => _database.Dispose();

    private async Task<Guid> SeedAsync(string username = "anna@example.com", AuthProvider provider = AuthProvider.Local, string? externalId = null)
    {
        var id = Guid.NewGuid();
        _database.Context.Users.Add(UserTestData.Create(id, username, provider, externalId));
        await _database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _database.Context.ChangeTracker.Clear();
        return id;
    }

    private async Task<User> ReadAsync(Guid id)
    {
        _database.Context.ChangeTracker.Clear();
        return await _database.Context.Users.AsNoTracking().SingleAsync(x => x.Id == id, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetByUsernameFindsExistingUser()
    {
        // Arrange
        var id = await SeedAsync();

        // Act
        var user = await _repository.GetByUsernameAsync("anna@example.com", TestContext.Current.CancellationToken);

        // Assert
        user.ShouldNotBeNull().Id.ShouldBe(id);
    }

    [Fact]
    public async Task GetByUsernameReturnsNullForUnknownUser()
    {
        // Arrange & Act
        var user = await _repository.GetByUsernameAsync("nobody@example.com", TestContext.Current.CancellationToken);

        // Assert
        user.ShouldBeNull();
    }

    [Fact]
    public async Task GetByExternalIdFindsEntraUser()
    {
        // Arrange
        var id = await SeedAsync(provider: AuthProvider.Entra, externalId: "oid-1");

        // Act
        var user = await _repository.GetByExternalIdAsync("oid-1", TestContext.Current.CancellationToken);

        // Assert
        user.ShouldNotBeNull().Id.ShouldBe(id);
    }

    [Fact]
    public async Task GetByExternalIdReturnsNullForUnknownId()
    {
        // Arrange
        await SeedAsync(provider: AuthProvider.Entra, externalId: "oid-1");

        // Act
        var user = await _repository.GetByExternalIdAsync("oid-2", TestContext.Current.CancellationToken);

        // Assert
        user.ShouldBeNull();
    }

    [Fact]
    public async Task DuplicateExternalIdIsRejectedByTheDatabase()
    {
        // Arrange
        await SeedAsync("a@example.com", AuthProvider.Entra, "oid-1");
        _database.Context.Users.Add(UserTestData.Create(Guid.NewGuid(), "b@example.com", AuthProvider.Entra, "oid-1"));

        // Act
        var act = () => _database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<DbUpdateException>(act);
    }

    [Fact]
    public async Task UsersWithoutExternalIdCanCoexist()
    {
        // Arrange
        await SeedAsync("a@example.com");
        _database.Context.Users.Add(UserTestData.Create(Guid.NewGuid(), "b@example.com"));

        // Act
        var saved = await _database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        saved.ShouldBe(1);
    }

    [Fact]
    public async Task RegisterFailedLoginBelowThresholdOnlyIncrementsTheCounter()
    {
        // Arrange
        var id = await SeedAsync();

        // Act
        await _repository.RegisterFailedLoginAsync(id, 5, LockoutEnd, TestContext.Current.CancellationToken);

        // Assert
        var user = await ReadAsync(id);
        user.AccessFailedCount.ShouldBe(1);
        user.IsLockedOut.ShouldBeFalse();
        user.LockoutEnd.ShouldBeNull();
    }

    [Fact]
    public async Task RegisterFailedLoginAtThresholdLocksTheAccount()
    {
        // Arrange
        var id = await SeedAsync();
        for (var i = 0; i < 4; i++)
        {
            await _repository.RegisterFailedLoginAsync(id, 5, LockoutEnd, TestContext.Current.CancellationToken);
        }

        // Act
        await _repository.RegisterFailedLoginAsync(id, 5, LockoutEnd, TestContext.Current.CancellationToken);

        // Assert
        var user = await ReadAsync(id);
        user.AccessFailedCount.ShouldBe(5);
        user.IsLockedOut.ShouldBeTrue();
        user.LockoutEnd.ShouldBe(LockoutEnd);
    }

    [Fact]
    public async Task RegisterFailedLoginForUnknownUserChangesNothing()
    {
        // Arrange
        var id = await SeedAsync();

        // Act
        await _repository.RegisterFailedLoginAsync(Guid.NewGuid(), 5, LockoutEnd, TestContext.Current.CancellationToken);

        // Assert
        (await ReadAsync(id)).AccessFailedCount.ShouldBe(0);
    }

    [Fact]
    public async Task RegisterSuccessfulLoginResetsStateAndKeepsPasswordHash()
    {
        // Arrange
        var id = await SeedAsync();
        await _repository.RegisterFailedLoginAsync(id, 1, LockoutEnd, TestContext.Current.CancellationToken);

        // Act
        await _repository.RegisterSuccessfulLoginAsync(id, LoginAt, null, TestContext.Current.CancellationToken);

        // Assert
        var user = await ReadAsync(id);
        user.AccessFailedCount.ShouldBe(0);
        user.IsLockedOut.ShouldBeFalse();
        user.LockoutEnd.ShouldBeNull();
        user.LastLoginAt.ShouldBe(LoginAt);
        user.PasswordHash.ShouldBe("hash");
    }

    [Fact]
    public async Task RegisterSuccessfulLoginStoresRehashedPassword()
    {
        // Arrange
        var id = await SeedAsync();

        // Act
        await _repository.RegisterSuccessfulLoginAsync(id, LoginAt, "new-hash", TestContext.Current.CancellationToken);

        // Assert
        (await ReadAsync(id)).PasswordHash.ShouldBe("new-hash");
    }

    [Fact]
    public async Task ClearLockoutResetsLockoutFieldsOnly()
    {
        // Arrange
        var id = await SeedAsync();
        await _repository.RegisterFailedLoginAsync(id, 1, LockoutEnd, TestContext.Current.CancellationToken);

        // Act
        await _repository.ClearLockoutAsync(id, TestContext.Current.CancellationToken);

        // Assert
        var user = await ReadAsync(id);
        user.IsLockedOut.ShouldBeFalse();
        user.AccessFailedCount.ShouldBe(0);
        user.LockoutEnd.ShouldBeNull();
        user.Username.ShouldBe("anna@example.com");
    }
}
