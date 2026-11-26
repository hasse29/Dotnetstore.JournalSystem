using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class UserAdministrationGuardTests : IDisposable
{
    private static readonly ActingUser Administrator = new(true, new HashSet<string>());

    private static readonly ActingUser Manager = new(
        false,
        new HashSet<string> { Permissions.Users.View, Permissions.Users.Edit, Permissions.WorkGroups.View });

    private readonly OrganizationTestDatabase _database;
    private readonly IUserAdministrationGuard _guard;
    private readonly Guid _administratorRole = Guid.NewGuid();
    private readonly Guid _lesserRole = Guid.NewGuid();
    private readonly Guid _greaterRole = Guid.NewGuid();

    public UserAdministrationGuardTests(SqlServerFixture fixture)
    {
        _database = new OrganizationTestDatabase(fixture);
        _guard = new UserAdministrationGuard(_database.UnitOfWork);

        _database.Context.Roles.AddRange(
            RoleTestData.Create(_administratorRole, RoleNames.Administrator, isSystem: true),
            RoleTestData.Create(_lesserRole, "Porter"),
            RoleTestData.Create(_greaterRole, "Chief"));
        _database.Context.RolePermissions.AddRange(
            new RolePermission(_lesserRole, Permissions.WorkGroups.View),
            new RolePermission(_greaterRole, Permissions.WorkGroups.Delete));
        _database.Context.SaveChanges();
        _database.Context.ChangeTracker.Clear();
    }

    public void Dispose() => _database.Dispose();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private async Task<Guid> SeedUserWithRoleAsync(Guid roleId)
    {
        var user = UserTestData.Create(Guid.NewGuid());
        _database.Context.Users.Add(user);
        _database.Context.Entry(user).Property(x => x.RoleId).CurrentValue = roleId;
        await _database.Context.SaveChangesAsync(Token);
        _database.Context.ChangeTracker.Clear();
        return user.Id;
    }

    [Fact]
    public async Task AnAdministratorMayAssignAndChangeAnything()
    {
        // Arrange
        var userId = await SeedUserWithRoleAsync(_administratorRole);

        // Act
        var assignment = await _guard.CheckAssignmentAsync(Administrator, _administratorRole, Token);
        var change = await _guard.CheckChangeAsync(Administrator, userId, _greaterRole, Token);

        // Assert
        assignment.ShouldBeNull();
        change.ShouldBeNull();
    }

    [Fact]
    public async Task OthersMayAssignARoleWithinTheirOwnRights()
    {
        // Act
        var result = await _guard.CheckAssignmentAsync(Manager, _lesserRole, Token);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task OthersMayNotAssignTheAdministratorRole()
    {
        // Act
        var result = await _guard.CheckAssignmentAsync(Manager, _administratorRole, Token);

        // Assert
        result.ShouldBe(UserAdministrationGuard.AdministratorRoleRefused);
    }

    [Fact]
    public async Task OthersMayNotAssignARoleWithRightsTheyLack()
    {
        // Act
        var result = await _guard.CheckAssignmentAsync(Manager, _greaterRole, Token);

        // Assert
        result.ShouldBe(UserAdministrationGuard.MoreRightsRefused);
    }

    [Fact]
    public async Task AnUnknownRoleIsLeftToValidation()
    {
        // Act
        var result = await _guard.CheckAssignmentAsync(Manager, Guid.NewGuid(), Token);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task OthersMayNotChangeOrDeleteAnAdministrator()
    {
        // Arrange
        var userId = await SeedUserWithRoleAsync(_administratorRole);

        // Act
        var change = await _guard.CheckChangeAsync(Manager, userId, _lesserRole, Token);
        var delete = await _guard.CheckChangeAsync(Manager, userId, null, Token);

        // Assert
        change.ShouldBe(UserAdministrationGuard.AdministratorUserRefused);
        delete.ShouldBe(UserAdministrationGuard.AdministratorUserRefused);
    }

    [Fact]
    public async Task OthersMayEditAUserWhoKeepsARoleBeyondTheirRights()
    {
        // Arrange
        var userId = await SeedUserWithRoleAsync(_greaterRole);

        // Act
        var result = await _guard.CheckChangeAsync(Manager, userId, _greaterRole, Token);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task OthersMayNotMoveAUserToARoleBeyondTheirRights()
    {
        // Arrange
        var userId = await SeedUserWithRoleAsync(_lesserRole);

        // Act
        var result = await _guard.CheckChangeAsync(Manager, userId, _greaterRole, Token);

        // Assert
        result.ShouldBe(UserAdministrationGuard.MoreRightsRefused);
    }

    [Fact]
    public async Task AnUnknownUserIsLeftToTheRequestToReport()
    {
        // Act
        var result = await _guard.CheckChangeAsync(Manager, Guid.NewGuid(), _lesserRole, Token);

        // Assert
        result.ShouldBeNull();
    }
}
