using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;

public sealed class RoleRepositoryTests : IDisposable
{
    private readonly OrganizationDataContext _context;
    private readonly RoleRepository _repository;

    public RoleRepositoryTests(SqlServerFixture fixture)
    {
        var options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new OrganizationDataContext(options);
        _context.Database.EnsureCreated();
        _repository = new RoleRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static Role CreateRole(string name = "Administrator")
    {
        return RoleTestData.Create(Guid.NewGuid(), name, "Description");
    }

    private async Task SeedAsync(params Role[] roles)
    {
        _context.Roles.AddRange(roles);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public void RepositoryIsAGenericRepositoryOfRole()
    {
        // Arrange

        // Act
        var isGenericRepository = _repository is IGenericRepository<Role>;

        // Assert
        isGenericRepository.ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsyncPersistsRoleWithAllValues()
    {
        // Arrange
        var role = CreateRole();

        // Act
        await _repository.AddAsync(role, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.Roles.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(role.Id);
        stored.Name.ShouldBe("Administrator");
        stored.Description.ShouldBe("Description");
    }

    [Fact]
    public async Task AddAsyncDoesNotPersistWhenChangesAreNotSaved()
    {
        // Arrange
        var role = CreateRole();

        // Act
        await _repository.AddAsync(role, TestContext.Current.CancellationToken);

        // Assert
        (await _context.Roles.AsNoTracking().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsRoleWhenItExists()
    {
        // Arrange
        var role = CreateRole();
        await SeedAsync(role);

        // Act
        var result = await _repository.GetByIdAsync(role.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(role.Name);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateRole());

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllRoles()
    {
        // Arrange
        await SeedAsync(CreateRole("A"), CreateRole("B"));

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Name).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoRolesExist()
    {
        // Arrange

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task UpdatePersistsChangedValues()
    {
        // Arrange
        var role = CreateRole();
        await SeedAsync(role);
        var changed = RoleTestData.Create(role.Id, "Renamed", "Changed");

        // Act
        _repository.Update(changed);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.Roles.SingleAsync(TestContext.Current.CancellationToken);
        stored.Name.ShouldBe("Renamed");
        stored.Description.ShouldBe("Changed");
    }

    [Fact]
    public async Task DeleteAsyncRemovesRoleWhenItExists()
    {
        // Arrange
        var role = CreateRole();
        await SeedAsync(role);

        // Act
        await _repository.DeleteAsync(role.Id, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.Roles.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteAsyncKeepsRolesWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateRole());

        // Act
        await _repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.Roles.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Theory]
    [InlineData("Nurse")]
    [InlineData("NURSE")]
    public async Task ExistsWithNameIsTrueForATakenNameWhateverItsCase(string name)
    {
        // Arrange
        IRoleRepository repository = _repository;
        await SeedAsync(CreateRole("Nurse"));

        // Act
        var exists = await repository.ExistsWithNameAsync(name, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsWithNameIsFalseForAFreeName()
    {
        // Arrange
        IRoleRepository repository = _repository;
        await SeedAsync(CreateRole("Nurse"));

        // Act
        var exists = await repository.ExistsWithNameAsync("Doctor", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsWithNameIgnoresTheExceptedRole()
    {
        // Arrange
        IRoleRepository repository = _repository;
        var role = CreateRole("Nurse");
        await SeedAsync(role);

        // Act
        var exists = await repository.ExistsWithNameAsync("Nurse", role.Id, TestContext.Current.CancellationToken);

        // Assert
        exists.ShouldBeFalse();
    }
}
