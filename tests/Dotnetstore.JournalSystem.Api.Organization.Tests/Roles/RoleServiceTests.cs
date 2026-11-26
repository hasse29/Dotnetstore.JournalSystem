using Dotnetstore.JournalSystem.Api.Organization.AccessRequests;
using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;

public sealed class RoleServiceTests : IDisposable
{
    private readonly DbContextOptions<OrganizationDataContext> _options;
    private readonly OrganizationDataContext _context;
    private readonly OrganizationUnitOfWork _unitOfWork;
    private readonly RoleService _service;

    public RoleServiceTests(SqlServerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new OrganizationDataContext(_options);
        _context.Database.EnsureCreated();
        _unitOfWork = new OrganizationUnitOfWork(_context, new AccessRequestRepository(_context), new OwnCompanyRepository(_context), new RoleRepository(_context), new UserRepository(_context), new WorkGroupRepository(_context));
        _service = new RoleService(_unitOfWork);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
        using var cleanup = new OrganizationDataContext(_options);
        cleanup.Database.EnsureDeleted();
    }

    private static EditRoleRequest CreateRequest(string name = "Administrator")
    {
        return new EditRoleRequest(name, "Description");
    }

    private async Task<Guid> SeedAsync(string name = "Seeded")
    {
        IRoleService service = _service;
        var result = await service.CreateAsync(CreateRequest(name), null, TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
        return result.Value.Id;
    }

    private async Task<int> CountStoredAsync()
    {
        return await _context.Roles.AsNoTracking().CountAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllRolesAsResponses()
    {
        // Arrange
        IRoleService service = _service;
        await SeedAsync("A");
        await SeedAsync("B");

        // Act
        var result = await service.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Name).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoRolesExist()
    {
        // Arrange
        IRoleService service = _service;

        // Act
        var result = await service.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsyncReturnsRoleAsResponse()
    {
        // Arrange
        IRoleService service = _service;
        var id = await SeedAsync("Find Me");

        // Act
        var result = await service.GetByIdAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(id);
        result.Value.Name.ShouldBe("Find Me");
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNotFoundForUnknownId()
    {
        // Arrange
        IRoleService service = _service;
        await SeedAsync();

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task CreateAsyncPersistsRoleAndReturnsResponse()
    {
        // Arrange
        IRoleService service = _service;
        var userId = Guid.NewGuid();

        // Act
        var result = await service.CreateAsync(CreateRequest(), userId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Administrator");
        result.Value.Id.ShouldNotBe(Guid.Empty);
        _context.ChangeTracker.Clear();
        var stored = await _context.Roles.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(result.Value.Id);
        stored.CreatedBy.ShouldBe(userId);
    }

    [Fact]
    public async Task CreateAsyncThrowsAndPersistsNothingForInvalidRequest()
    {
        // Arrange
        IRoleService service = _service;

        // Act
        var act = async () => await service.CreateAsync(CreateRequest(""), null, TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
        (await CountStoredAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task UpdateAsyncPersistsChangesAndReturnsResponse()
    {
        // Arrange
        IRoleService service = _service;
        var id = await SeedAsync();
        var editor = Guid.NewGuid();

        // Act
        var result = await service.UpdateAsync(id, CreateRequest("Renamed"), editor, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe("Renamed");
        _context.ChangeTracker.Clear();
        var stored = await _context.Roles.SingleAsync(TestContext.Current.CancellationToken);
        stored.Name.ShouldBe("Renamed");
        stored.LastUpdatedBy.ShouldBe(editor);
        stored.LastUpdatedDate.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateAsyncReturnsNotFoundForUnknownId()
    {
        // Arrange
        IRoleService service = _service;

        // Act
        var result = await service.UpdateAsync(Guid.NewGuid(), CreateRequest(), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task UpdateAsyncKeepsStoredValuesWhenRequestIsInvalid()
    {
        // Arrange
        IRoleService service = _service;
        var id = await SeedAsync("Original");

        // Act
        var act = async () => await service.UpdateAsync(id, CreateRequest(""), null, TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
        _context.ChangeTracker.Clear();
        (await _context.Roles.SingleAsync(TestContext.Current.CancellationToken)).Name.ShouldBe("Original");
    }

    [Fact]
    public async Task DeleteAsyncRemovesExistingRole()
    {
        // Arrange
        IRoleService service = _service;
        var id = await SeedAsync();

        // Act
        var result = await service.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        (await CountStoredAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteAsyncReturnsNotFoundAndKeepsDataForUnknownId()
    {
        // Arrange
        IRoleService service = _service;
        await SeedAsync();

        // Act
        var result = await service.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
        (await CountStoredAsync()).ShouldBe(1);
    }

    private async Task<Guid> SeedSystemRoleAsync()
    {
        var role = RoleTestData.Create(Guid.NewGuid(), "Administrator", "Full access", isSystem: true);
        _context.Roles.Add(role);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
        return role.Id;
    }

    private async Task<Role> ReadAsync(Guid id)
    {
        _context.ChangeTracker.Clear();
        return await _context.Roles.AsNoTracking().SingleAsync(x => x.Id == id, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CreateAsyncTrimsTheName()
    {
        // Arrange
        IRoleService service = _service;

        // Act
        var result = await service.CreateAsync(CreateRequest("  Nurse  "), null, TestContext.Current.CancellationToken);

        // Assert
        (await ReadAsync(result.Value.Id)).Name.ShouldBe("Nurse");
    }

    [Theory]
    [InlineData("Nurse")]
    [InlineData("nurse")]
    [InlineData("  Nurse ")]
    public async Task CreateAsyncRefusesATakenName(string name)
    {
        // Arrange
        IRoleService service = _service;
        await SeedAsync("Nurse");

        // Act
        var result = await service.CreateAsync(CreateRequest(name), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Conflict);
        (await CountStoredAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task UpdateAsyncRefusesTheNameOfAnotherRole()
    {
        // Arrange
        IRoleService service = _service;
        await SeedAsync("Nurse");
        var id = await SeedAsync("Doctor");

        // Act
        var result = await service.UpdateAsync(id, CreateRequest("NURSE"), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Conflict);
        (await ReadAsync(id)).Name.ShouldBe("Doctor");
    }

    [Fact]
    public async Task UpdateAsyncAllowsChangingTheCaseOfItsOwnName()
    {
        // Arrange
        IRoleService service = _service;
        var id = await SeedAsync("nurse");

        // Act
        var result = await service.UpdateAsync(id, CreateRequest("Nurse"), null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        (await ReadAsync(id)).Name.ShouldBe("Nurse");
    }

    [Fact]
    public async Task UpdateAsyncStillEditsARoleThatAlreadySharesItsName()
    {
        // Arrange
        IRoleService service = _service;
        var id = Guid.NewGuid();
        _context.Roles.AddRange(RoleTestData.Create(id, "Nurse"), RoleTestData.Create(Guid.NewGuid(), "Nurse"));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();

        // Act
        var result = await service.UpdateAsync(id, new EditRoleRequest("Nurse", "Changed"), null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        (await ReadAsync(id)).Description.ShouldBe("Changed");
    }

    [Theory]
    [InlineData("Boss")]
    [InlineData("administrator")]
    public async Task UpdateAsyncRefusesToRenameASystemRole(string name)
    {
        // Arrange
        IRoleService service = _service;
        var id = await SeedSystemRoleAsync();

        // Act
        var result = await service.UpdateAsync(id, CreateRequest(name), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Conflict);
        (await ReadAsync(id)).Name.ShouldBe("Administrator");
    }

    [Fact]
    public async Task UpdateAsyncChangesTheDescriptionOfASystemRoleAndKeepsItASystemRole()
    {
        // Arrange
        IRoleService service = _service;
        var id = await SeedSystemRoleAsync();

        // Act
        var result = await service.UpdateAsync(id, new EditRoleRequest("Administrator", "Changed"), null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var stored = await ReadAsync(id);
        stored.Description.ShouldBe("Changed");
        stored.IsSystem.ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteAsyncRefusesASystemRole()
    {
        // Arrange
        IRoleService service = _service;
        var id = await SeedSystemRoleAsync();

        // Act
        var result = await service.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Conflict);
        (await CountStoredAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task DeleteAsyncRefusesARoleThatAUserHas()
    {
        // Arrange
        IRoleService service = _service;
        var user = UserTestData.Create(Guid.NewGuid());
        _context.Roles.Add(RoleTestData.Create(user.RoleId, "Nurse"));
        _context.Users.Add(user);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();

        // Act
        var result = await service.DeleteAsync(user.RoleId, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Conflict);
        (await CountStoredAsync()).ShouldBe(1);
    }

    private static EditRoleRequest WithRights(string name, params string[] permissions) => new(name, "Description", permissions);

    [Fact]
    public async Task CreateAsyncStoresTheRightsAndWhatTheyRequire()
    {
        // Arrange
        IRoleService service = _service;

        // Act
        var result = await service.CreateAsync(WithRights("Nurse", Permissions.WorkGroups.Edit), null, TestContext.Current.CancellationToken);

        // Assert
        string[] expected = [Permissions.WorkGroups.View, Permissions.WorkGroups.Edit];
        result.Value.Permissions.ShouldBe(expected);
        _context.ChangeTracker.Clear();
        var fetched = await service.GetByIdAsync(result.Value.Id, TestContext.Current.CancellationToken);
        fetched.Value.Permissions.ShouldBe(expected);
        (await service.GetAllAsync(TestContext.Current.CancellationToken)).Single().Permissions.ShouldBe(expected);
    }

    [Fact]
    public async Task CreateAsyncGivesNoRightsUnlessAsked()
    {
        // Arrange
        IRoleService service = _service;

        // Act
        var result = await service.CreateAsync(CreateRequest("Nurse"), null, TestContext.Current.CancellationToken);

        // Assert
        result.Value.Permissions.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateAsyncChangesRightsOnlyWhenTheyAreSent(bool send)
    {
        // Arrange
        IRoleService service = _service;
        var id = (await service.CreateAsync(WithRights("Nurse", Permissions.Roles.View), null, TestContext.Current.CancellationToken)).Value.Id;
        _context.ChangeTracker.Clear();
        var request = new EditRoleRequest("Nurse", "Changed", send ? [] : null);

        // Act
        var result = await service.UpdateAsync(id, request, null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Permissions.ShouldNotBeNull().Count.ShouldBe(send ? 0 : 1);
    }

    [Fact]
    public async Task UpdateAsyncStoresNoRightsForASystemRoleWhichHasThemAll()
    {
        // Arrange
        IRoleService service = _service;
        var id = await SeedSystemRoleAsync();

        // Act
        var result = await service.UpdateAsync(id, WithRights(RoleNames.Administrator, Permissions.Roles.View), null, TestContext.Current.CancellationToken);

        // Assert
        result.Value.Permissions.ShouldBe(Permissions.All);
        (await _context.RolePermissions.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }
}
