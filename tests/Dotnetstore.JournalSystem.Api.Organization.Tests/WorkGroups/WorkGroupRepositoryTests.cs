using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.WorkGroups;

public sealed class WorkGroupRepositoryTests : IDisposable
{
    private readonly OrganizationDataContext _context;
    private readonly WorkGroupRepository _repository;

    public WorkGroupRepositoryTests(SqlServerFixture fixture)
    {
        var options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new OrganizationDataContext(options);
        _context.Database.EnsureCreated();
        _repository = new WorkGroupRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static WorkGroup CreateWorkGroup(string name = "Administrator")
    {
        return WorkGroupTestData.Create(Guid.NewGuid(), name, "Description");
    }

    private async Task SeedAsync(params WorkGroup[] workGroups)
    {
        _context.WorkGroups.AddRange(workGroups);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public void RepositoryIsAGenericRepositoryOfWorkGroup()
    {
        // Arrange

        // Act
        var isGenericRepository = _repository is IGenericRepository<WorkGroup>;

        // Assert
        isGenericRepository.ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsyncPersistsWorkGroupWithAllValues()
    {
        // Arrange
        var workGroup = CreateWorkGroup();

        // Act
        await _repository.AddAsync(workGroup, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.WorkGroups.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(workGroup.Id);
        stored.Name.ShouldBe("Administrator");
        stored.Description.ShouldBe("Description");
    }

    [Fact]
    public async Task AddAsyncDoesNotPersistWhenChangesAreNotSaved()
    {
        // Arrange
        var workGroup = CreateWorkGroup();

        // Act
        await _repository.AddAsync(workGroup, TestContext.Current.CancellationToken);

        // Assert
        (await _context.WorkGroups.AsNoTracking().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsWorkGroupWhenItExists()
    {
        // Arrange
        var workGroup = CreateWorkGroup();
        await SeedAsync(workGroup);

        // Act
        var result = await _repository.GetByIdAsync(workGroup.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(workGroup.Name);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateWorkGroup());

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllWorkGroups()
    {
        // Arrange
        await SeedAsync(CreateWorkGroup("A"), CreateWorkGroup("B"));

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Name).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoWorkGroupsExist()
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
        var workGroup = CreateWorkGroup();
        await SeedAsync(workGroup);
        var changed = WorkGroupTestData.Create(workGroup.Id, "Renamed", "Changed");

        // Act
        _repository.Update(changed);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.WorkGroups.SingleAsync(TestContext.Current.CancellationToken);
        stored.Name.ShouldBe("Renamed");
        stored.Description.ShouldBe("Changed");
    }

    [Fact]
    public async Task DeleteAsyncRemovesWorkGroupWhenItExists()
    {
        // Arrange
        var workGroup = CreateWorkGroup();
        await SeedAsync(workGroup);

        // Act
        await _repository.DeleteAsync(workGroup.Id, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.WorkGroups.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteAsyncKeepsWorkGroupsWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateWorkGroup());

        // Act
        await _repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.WorkGroups.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }
}
