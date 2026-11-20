using System.Security.Claims;
using Dotnetstore.JournalSystem.Api.Organization.Authentication;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Authentication;

public sealed class EntraUserClaimsTransformationTests : IDisposable
{
    private const string ObjectId = "11111111-2222-3333-4444-555555555555";

    private readonly OrganizationTestDatabase _database;
    private readonly ServiceProvider _provider;
    private readonly IClaimsTransformation _transformation;
    private readonly Guid _roleId = Guid.NewGuid();
    private readonly Guid _ownCompanyId = Guid.NewGuid();

    public EntraUserClaimsTransformationTests(SqlServerFixture fixture)
    {
        _database = new OrganizationTestDatabase(fixture);
        _provider = new ServiceCollection()
            .AddSingleton<IOrganizationUnitOfWork>(_database.UnitOfWork)
            .BuildServiceProvider();
        _transformation = new EntraUserClaimsTransformation(_provider);

        _database.Context.Roles.Add(RoleTestData.Create(_roleId, "Nurse"));
        _database.Context.SaveChanges();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _database.Dispose();
    }

    private static ClaimsPrincipal Authenticated(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "test"));

    private async Task SeedEntraUserAsync(bool approved = true)
    {
        var request = new CreateUserRequest("Anna", null, "Svensson", "anna@example.com", _ownCompanyId, _roleId, AuthProvider.Entra, null);
        var user = UserBuilder
            .Create()
            .WithCreateValues(request, string.Empty, ObjectId, null)
            .SetAccountIsApproved(approved)
            .Build();
        _database.Context.Users.Add(user);
        await _database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _database.Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task KnownEntraUserGetsRoleUserIdAndOwnCompanyClaims()
    {
        // Arrange
        await SeedEntraUserAsync();
        var principal = Authenticated(new Claim("oid", ObjectId));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.FindFirst(AuthClaimTypes.Role)!.Value.ShouldBe("Nurse");
        result.FindFirst(AuthClaimTypes.UserId).ShouldNotBeNull();
        result.FindFirst(AuthClaimTypes.OwnCompanyId)!.Value.ShouldBe(_ownCompanyId.ToString());
    }

    [Fact]
    public async Task ObjectIdentifierLongClaimNameIsAlsoRecognised()
    {
        // Arrange
        await SeedEntraUserAsync();
        var principal = Authenticated(new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", ObjectId));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.FindFirst(AuthClaimTypes.Role)!.Value.ShouldBe("Nurse");
    }

    [Fact]
    public async Task RoleClaimFromTheTokenIsReplacedNotTrusted()
    {
        // Arrange
        await SeedEntraUserAsync();
        var principal = Authenticated(new Claim("oid", ObjectId), new Claim(AuthClaimTypes.Role, RoleNames.Administrator));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.FindAll(AuthClaimTypes.Role).Select(x => x.Value).ShouldBe(["Nurse"]);
    }

    [Fact]
    public async Task UnknownEntraUserGetsNoRoleAndTokenRoleIsRemoved()
    {
        // Arrange
        var principal = Authenticated(new Claim("oid", ObjectId), new Claim(AuthClaimTypes.Role, RoleNames.Administrator));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.FindAll(AuthClaimTypes.Role).ShouldBeEmpty();
        result.FindFirst(AuthClaimTypes.UserId).ShouldBeNull();
    }

    [Fact]
    public async Task UnapprovedEntraUserGetsNoClaims()
    {
        // Arrange
        await SeedEntraUserAsync(approved: false);
        var principal = Authenticated(new Claim("oid", ObjectId));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.FindFirst(AuthClaimTypes.Role).ShouldBeNull();
        result.FindFirst(AuthClaimTypes.UserId).ShouldBeNull();
    }

    [Fact]
    public async Task PrincipalWithoutObjectIdIsReturnedUnchanged()
    {
        // Arrange
        var principal = Authenticated(new Claim(AuthClaimTypes.UserId, Guid.NewGuid().ToString()));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.ShouldBeSameAs(principal);
    }

    [Fact]
    public async Task AlreadyTransformedPrincipalIsReturnedUnchanged()
    {
        // Arrange
        var principal = Authenticated(new Claim("oid", ObjectId), new Claim(AuthClaimTypes.UserId, Guid.NewGuid().ToString()));

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        result.ShouldBeSameAs(principal);
    }

    [Fact]
    public async Task AnonymousPrincipalIsReturnedUnchangedWithoutTouchingTheDatabase()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        await using var emptyProvider = new ServiceCollection().BuildServiceProvider();
        IClaimsTransformation transformation = new EntraUserClaimsTransformation(emptyProvider);

        // Act
        var result = await transformation.TransformAsync(principal);

        // Assert
        result.ShouldBeSameAs(principal);
    }
}
