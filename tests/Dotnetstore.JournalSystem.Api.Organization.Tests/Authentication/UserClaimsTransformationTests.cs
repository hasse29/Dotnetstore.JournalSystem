using System.Security.Claims;
using Dotnetstore.JournalSystem.Api.Organization.Authentication;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Authentication;

public sealed class UserClaimsTransformationTests : IDisposable
{
    private const string ObjectId = "11111111-2222-3333-4444-555555555555";

    private readonly OrganizationTestDatabase _database;
    private readonly ServiceProvider _provider;
    private readonly IClaimsTransformation _transformation;
    private readonly Guid _roleId = Guid.NewGuid();
    private readonly Guid _ownCompanyId = Guid.NewGuid();

    public UserClaimsTransformationTests(SqlServerFixture fixture)
    {
        _database = new OrganizationTestDatabase(fixture);
        _provider = new ServiceCollection()
            .AddSingleton<IOrganizationUnitOfWork>(_database.UnitOfWork)
            .BuildServiceProvider();
        _transformation = new UserClaimsTransformation(_provider);

        _database.Context.Roles.Add(RoleTestData.Create(_roleId, "Nurse"));
        _database.Context.RolePermissions.Add(new RolePermission(_roleId, Permissions.WorkGroups.View));
        _database.Context.SaveChanges();
        _database.Context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _database.Dispose();
    }

    private static ClaimsPrincipal Authenticated(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "test"));

    private static ClaimsPrincipal Password(Guid userId, params Claim[] claims) =>
        Authenticated([new Claim(AuthClaimTypes.UserId, userId.ToString()), .. claims]);

    private static string[] Values(ClaimsPrincipal principal, string type) =>
        [.. principal.FindAll(type).Select(x => x.Value)];

    private async Task<Guid> SeedUserAsync(
        AuthProvider provider = AuthProvider.Local,
        bool approved = true,
        bool lockedOut = false,
        Guid? roleId = null)
    {
        var request = new CreateUserRequest("Anna", null, "Svensson", "anna@example.com", _ownCompanyId, roleId ?? _roleId, provider, null);
        var user = UserBuilder
            .Create()
            .WithCreateValues(request, string.Empty, provider == AuthProvider.Entra ? ObjectId : null, null)
            .SetAccountIsApproved(approved)
            .Build();
        _database.Context.Users.Add(user);
        await _database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        if (lockedOut)
        {
            await _database.Context.Users
                .Where(x => x.Id == user.Id)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsLockedOut, true), TestContext.Current.CancellationToken);
        }

        _database.Context.ChangeTracker.Clear();
        return user.Id;
    }

    private static void ShouldHaveNoRights(ClaimsPrincipal principal)
    {
        principal.FindAll(AuthClaimTypes.UserId).ShouldBeEmpty();
        principal.FindAll(AuthClaimTypes.Role).ShouldBeEmpty();
        principal.FindAll(AuthClaimTypes.Permission).ShouldBeEmpty();
    }

    [Fact]
    public async Task PasswordUserGetsTheRoleAndRightsTheyHaveNow()
    {
        // Arrange
        var userId = await SeedUserAsync();
        var principal = Password(userId, new Claim(AuthClaimTypes.Role, "What it was at sign-in"));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        Values(result, AuthClaimTypes.UserId).ShouldBe([userId.ToString()]);
        Values(result, AuthClaimTypes.OwnCompanyId).ShouldBe([_ownCompanyId.ToString()]);
        Values(result, AuthClaimTypes.Role).ShouldBe(["Nurse"]);
        Values(result, AuthClaimTypes.Permission).ShouldBe([Permissions.WorkGroups.View]);
    }

    [Theory]
    [InlineData("oid")]
    [InlineData("http://schemas.microsoft.com/identity/claims/objectidentifier")]
    public async Task MicrosoftUserGetsTheirRoleAndRights(string claimType)
    {
        // Arrange
        var userId = await SeedUserAsync(AuthProvider.Entra);

        // Act
        var result = await _transformation.TransformAsync(Authenticated(new Claim(claimType, ObjectId)));

        // Assert
        Values(result, AuthClaimTypes.UserId).ShouldBe([userId.ToString()]);
        Values(result, AuthClaimTypes.Role).ShouldBe(["Nurse"]);
        Values(result, AuthClaimTypes.Permission).ShouldBe([Permissions.WorkGroups.View]);
    }

    [Fact]
    public async Task AnAdministratorGetsEveryRight()
    {
        // Arrange
        var administrator = Guid.NewGuid();
        _database.Context.Roles.Add(RoleTestData.Create(administrator, RoleNames.Administrator, isSystem: true));
        var userId = await SeedUserAsync(roleId: administrator);

        // Act
        var result = await _transformation.TransformAsync(Password(userId));

        // Assert
        Values(result, AuthClaimTypes.Permission).ShouldBe(Permissions.All, ignoreOrder: true);
    }

    [Fact]
    public async Task RoleAndRightsInATokenAreNotTrusted()
    {
        // Arrange
        var principal = Authenticated(
            new Claim("oid", ObjectId),
            new Claim(AuthClaimTypes.Role, RoleNames.Administrator),
            new Claim(AuthClaimTypes.Permission, Permissions.Users.Delete));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        ShouldHaveNoRights(result);
    }

    [Fact]
    public async Task ADeletedPasswordUserLosesEverything()
    {
        // Arrange
        var principal = Password(Guid.NewGuid(), new Claim(AuthClaimTypes.Role, RoleNames.Administrator));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        ShouldHaveNoRights(result);
    }

    [Fact]
    public async Task AUserIdThatIsNotAnIdGivesNothing()
    {
        // Act
        var result = await _transformation.TransformAsync(Authenticated(new Claim(AuthClaimTypes.UserId, "not-a-guid")));

        // Assert
        ShouldHaveNoRights(result);
    }

    [Theory]
    [InlineData(AuthProvider.Local)]
    [InlineData(AuthProvider.Entra)]
    public async Task AnUnapprovedUserGetsNothing(AuthProvider provider)
    {
        // Arrange
        var userId = await SeedUserAsync(provider, approved: false);
        var principal = provider == AuthProvider.Local ? Password(userId) : Authenticated(new Claim("oid", ObjectId));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        ShouldHaveNoRights(result);
    }

    [Fact]
    public async Task ALockedMicrosoftUserGetsNothing()
    {
        // Arrange
        await SeedUserAsync(AuthProvider.Entra, lockedOut: true);

        // Act
        var result = await _transformation.TransformAsync(Authenticated(new Claim("oid", ObjectId)));

        // Assert
        ShouldHaveNoRights(result);
    }

    [Fact]
    public async Task ALockedPasswordUserKeepsTheSessionThatIsAlreadyOpen()
    {
        // Arrange
        var userId = await SeedUserAsync(lockedOut: true);

        // Act
        var result = await _transformation.TransformAsync(Password(userId));

        // Assert
        Values(result, AuthClaimTypes.Role).ShouldBe(["Nurse"]);
    }

    [Fact]
    public async Task AResolvedPrincipalIsReturnedUnchanged()
    {
        // Arrange
        var userId = await SeedUserAsync();
        var resolved = await _transformation.TransformAsync(Password(userId));

        // Act
        var result = await _transformation.TransformAsync(resolved);

        // Assert
        result.ShouldBeSameAs(resolved);
    }

    [Fact]
    public async Task AnonymousPrincipalIsReturnedUnchangedWithoutTouchingTheDatabase()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        await using var emptyProvider = new ServiceCollection().BuildServiceProvider();
        IClaimsTransformation transformation = new UserClaimsTransformation(emptyProvider);

        // Act
        var result = await transformation.TransformAsync(principal);

        // Assert
        result.ShouldBeSameAs(principal);
    }
}
