using Dotnetstore.JournalSystem.Api.Organization.Seeding;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.Security.Extensions;
using Dotnetstore.Security.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Seeding;

public sealed class OrganizationBootstrapperTests : IDisposable
{
    private readonly OrganizationTestDatabase _database;
    private readonly ServiceProvider _provider;
    private readonly IHashHandler _hashHandler;

    public OrganizationBootstrapperTests(SqlServerFixture fixture)
    {
        _database = new OrganizationTestDatabase(fixture);
        _provider = new ServiceCollection().AddSecurity().BuildServiceProvider();
        _hashHandler = _provider.GetRequiredService<IHashHandler>();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _database.Dispose();
    }

    private async Task SeedSystemOwnCompanyAsync()
    {
        var company = OwnCompanyTestData.Create(Guid.NewGuid(), "Dotnetstore AB", isSystem: true);
        _database.Context.OwnCompanies.Add(company);
        await _database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private static BootstrapAdminOptions LocalOptions() => new()
    {
        Username = "admin@example.com",
        Password = "correct-horse-battery-staple"
    };

    private Task<bool> BootstrapAsync(BootstrapAdminOptions? options) =>
        OrganizationBootstrapper.BootstrapAdminAsync(_database.Context, _hashHandler, options, TestContext.Current.CancellationToken);

    [Fact]
    public async Task CreatesSystemAdministratorRoleAndLocalAdminUser()
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();

        // Act
        var created = await BootstrapAsync(LocalOptions());

        // Assert
        created.ShouldBeTrue();
        var role = await _database.Context.Roles.SingleAsync(TestContext.Current.CancellationToken);
        role.Name.ShouldBe(RoleNames.Administrator);
        role.IsSystem.ShouldBeTrue();
        var user = await _database.Context.Users.SingleAsync(TestContext.Current.CancellationToken);
        user.IsSystem.ShouldBeTrue();
        user.RoleId.ShouldBe(role.Id);
        user.AuthProvider.ShouldBe(AuthProvider.Local);
        _hashHandler.Verify("correct-horse-battery-staple", user.PasswordHash).ShouldBeTrue();
    }

    [Fact]
    public async Task AcceptsAPasswordShorterThanTheUserPasswordMinimum()
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();
        var options = new BootstrapAdminOptions { Username = "admin@example.com", Password = "1234567890" };

        // Act
        var created = await BootstrapAsync(options);

        // Assert
        created.ShouldBeTrue();
        var user = await _database.Context.Users.SingleAsync(TestContext.Current.CancellationToken);
        _hashHandler.Verify("1234567890", user.PasswordHash).ShouldBeTrue();
    }

    [Fact]
    public async Task DoesNothingWhenThePasswordIsLongerThanTheMaximum()
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();
        var options = new BootstrapAdminOptions { Username = "admin@example.com", Password = new string('a', 129) };

        // Act
        var created = await BootstrapAsync(options);

        // Assert
        created.ShouldBeFalse();
        (await _database.Context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task CreatesEntraLinkedAdminWithoutPassword()
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();
        var options = new BootstrapAdminOptions { Username = "admin@example.com", EntraObjectId = "oid-admin" };

        // Act
        var created = await BootstrapAsync(options);

        // Assert
        created.ShouldBeTrue();
        var user = await _database.Context.Users.SingleAsync(TestContext.Current.CancellationToken);
        user.AuthProvider.ShouldBe(AuthProvider.Entra);
        user.ExternalId.ShouldBe("oid-admin");
        user.PasswordHash.ShouldBeEmpty();
    }

    [Fact]
    public async Task IsIdempotentAndDoesNotDuplicateRoleOrUser()
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();
        await BootstrapAsync(LocalOptions());

        // Act
        var second = await BootstrapAsync(LocalOptions());

        // Assert
        second.ShouldBeFalse();
        (await _database.Context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
        (await _database.Context.Roles.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task ReusesExistingSystemAdministratorRole()
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();
        await BootstrapAsync(LocalOptions());
        var options = new BootstrapAdminOptions { Username = "second@example.com", Password = "correct-horse-battery-staple" };

        // Act
        await BootstrapAsync(options);

        // Assert
        (await _database.Context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(2);
        (await _database.Context.Roles.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task DoesNothingWithoutSystemOwnCompany()
    {
        // Arrange & Act
        var created = await BootstrapAsync(LocalOptions());

        // Assert
        created.ShouldBeFalse();
        (await _database.Context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "correct-horse-battery-staple")]
    [InlineData("admin@example.com", null)]
    [InlineData("admin@example.com", "")]
    [InlineData("admin@example.com", "   ")]
    public async Task DoesNothingWhenUsernameOrPasswordIsMissingOrWeak(string? username, string? password)
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();
        var options = new BootstrapAdminOptions { Username = username, Password = password };

        // Act
        var created = await BootstrapAsync(options);

        // Assert
        created.ShouldBeFalse();
        (await _database.Context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task DoesNothingWhenOptionsAreMissing()
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();

        // Act
        var created = await BootstrapAsync(null);

        // Assert
        created.ShouldBeFalse();
    }

    [Fact]
    public async Task CreatesALocalAdminThatIsAlsoLinkedToEntraWhenBothAreConfigured()
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();
        var options = LocalOptions();
        options.EntraObjectId = "11111111-1111-1111-1111-111111111111";

        // Act
        var created = await BootstrapAsync(options);

        // Assert
        created.ShouldBeTrue();
        var user = await _database.Context.Users.SingleAsync(TestContext.Current.CancellationToken);
        user.AuthProvider.ShouldBe(AuthProvider.Local);
        user.ExternalId.ShouldBe("11111111-1111-1111-1111-111111111111");
        _hashHandler.Verify("correct-horse-battery-staple", user.PasswordHash).ShouldBeTrue();
    }

    [Fact]
    public async Task LinksAnExistingLocalAdminToEntraWithoutChangingThePassword()
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();
        await BootstrapAsync(LocalOptions());
        var options = LocalOptions();
        options.Password = null;
        options.EntraObjectId = "22222222-2222-2222-2222-222222222222";

        // Act
        var changed = await BootstrapAsync(options);

        // Assert
        changed.ShouldBeTrue();
        var user = await _database.Context.Users.SingleAsync(TestContext.Current.CancellationToken);
        user.ExternalId.ShouldBe("22222222-2222-2222-2222-222222222222");
        user.AuthProvider.ShouldBe(AuthProvider.Local);
        _hashHandler.Verify("correct-horse-battery-staple", user.PasswordHash).ShouldBeTrue();
    }

    [Fact]
    public async Task DoesNotReplaceAnEntraLinkThatIsAlreadySet()
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();
        var first = LocalOptions();
        first.EntraObjectId = "33333333-3333-3333-3333-333333333333";
        await BootstrapAsync(first);
        var second = LocalOptions();
        second.EntraObjectId = "44444444-4444-4444-4444-444444444444";

        // Act
        var changed = await BootstrapAsync(second);

        // Assert
        changed.ShouldBeFalse();
        var user = await _database.Context.Users.SingleAsync(TestContext.Current.CancellationToken);
        user.ExternalId.ShouldBe("33333333-3333-3333-3333-333333333333");
    }

    [Fact]
    public async Task DoesNotLinkAnExistingAdminWhenAnotherUserOwnsTheEntraIdentity()
    {
        // Arrange
        await SeedSystemOwnCompanyAsync();
        var owner = new BootstrapAdminOptions { Username = "owner@example.com", EntraObjectId = "55555555-5555-5555-5555-555555555555" };
        await BootstrapAsync(owner);
        await BootstrapAsync(LocalOptions());
        var options = LocalOptions();
        options.Password = null;
        options.EntraObjectId = "55555555-5555-5555-5555-555555555555";

        // Act
        var changed = await BootstrapAsync(options);

        // Assert
        changed.ShouldBeFalse();
        var admin = await _database.Context.Users.SingleAsync(x => x.Username == "admin@example.com", TestContext.Current.CancellationToken);
        admin.ExternalId.ShouldBeNull();
    }
}
