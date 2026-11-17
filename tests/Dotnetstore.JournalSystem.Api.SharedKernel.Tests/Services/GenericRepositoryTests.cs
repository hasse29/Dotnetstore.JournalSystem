using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.SharedKernel.Tests.Services;

public sealed class GenericRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TestDbContext _context;
    private readonly IGenericRepository<TestItem> _repository;

    public GenericRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TestDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new GenericRepository<TestItem>(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task SeedAsync(params TestItem[] items)
    {
        _context.Items.AddRange(items);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllEntities()
    {
        // Arrange
        await SeedAsync(new TestItem { Name = "A" }, new TestItem { Name = "B" });

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Name).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoEntitiesExist()
    {
        // Arrange

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllAsyncThrowsWhenTokenIsCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        var act = async () => await _repository.GetAllAsync(cts.Token);

        // Assert
        await Should.ThrowAsync<OperationCanceledException>(act);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsEntityWhenItExists()
    {
        // Arrange
        var item = new TestItem { Name = "A" };
        await SeedAsync(item);

        // Act
        var result = await _repository.GetByIdAsync(item.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("A");
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullWhenItDoesNotExist()
    {
        // Arrange

        // Act
        var result = await _repository.GetByIdAsync(999, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task AddAsyncPersistsEntityWhenChangesAreSaved()
    {
        // Arrange
        var item = new TestItem { Name = "A" };

        // Act
        await _repository.AddAsync(item, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        (await _context.Items.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task AddAsyncDoesNotPersistEntityWhenChangesAreNotSaved()
    {
        // Arrange
        var item = new TestItem { Name = "A" };

        // Act
        await _repository.AddAsync(item, TestContext.Current.CancellationToken);

        // Assert
        _context.Entry(item).State.ShouldBe(EntityState.Added);
        (await _context.Items.AsNoTracking().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task AddAsyncPersistsAllEntitiesWhenGivenCollection()
    {
        // Arrange
        List<TestItem> items = [new() { Name = "A" }, new() { Name = "B" }];

        // Act
        await _repository.AddAsync(items, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        (await _context.Items.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(2);
    }

    [Fact]
    public async Task AddAsyncAddsNothingWhenCollectionIsEmpty()
    {
        // Arrange

        // Act
        await _repository.AddAsync(new List<TestItem>(), TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.Items.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task UpdatePersistsChangesWhenChangesAreSaved()
    {
        // Arrange
        var item = new TestItem { Name = "A" };
        await SeedAsync(item);
        var detached = new TestItem { Id = item.Id, Name = "Changed" };

        // Act
        _repository.Update(detached);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.Items.SingleAsync(TestContext.Current.CancellationToken);
        stored.Name.ShouldBe("Changed");
    }

    [Fact]
    public async Task UpdateMarksEntityAsModified()
    {
        // Arrange
        var item = new TestItem { Id = 1, Name = "A" };

        // Act
        _repository.Update(item);

        // Assert
        _context.Entry(item).State.ShouldBe(EntityState.Modified);
    }

    [Fact]
    public async Task UpdatePersistsAllEntitiesWhenGivenCollection()
    {
        // Arrange
        var a = new TestItem { Name = "A" };
        var b = new TestItem { Name = "B" };
        await SeedAsync(a, b);
        List<TestItem> changed = [new() { Id = a.Id, Name = "A2" }, new() { Id = b.Id, Name = "B2" }];

        // Act
        _repository.Update(changed);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var names = await _context.Items.Select(x => x.Name).ToListAsync(TestContext.Current.CancellationToken);
        names.ShouldBe(["A2", "B2"], ignoreOrder: true);
    }

    [Fact]
    public async Task DeleteAsyncRemovesEntityWhenItExists()
    {
        // Arrange
        var item = new TestItem { Name = "A" };
        await SeedAsync(item);

        // Act
        await _repository.DeleteAsync(item.Id, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.Items.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteAsyncDoesNothingWhenEntityDoesNotExist()
    {
        // Arrange
        await SeedAsync(new TestItem { Name = "A" });

        // Act
        await _repository.DeleteAsync(999, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.Items.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task DeleteAsyncRemovesOnlyExistingEntitiesWhenGivenIds()
    {
        // Arrange
        var a = new TestItem { Name = "A" };
        var b = new TestItem { Name = "B" };
        var c = new TestItem { Name = "C" };
        await SeedAsync(a, b, c);
        List<object> ids = [a.Id, b.Id, 999];

        // Act
        await _repository.DeleteAsync(ids, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var remaining = await _context.Items.ToListAsync(TestContext.Current.CancellationToken);
        remaining.Select(x => x.Name).ShouldBe(["C"]);
    }

    [Fact]
    public async Task DeleteAsyncRemovesNothingWhenIdCollectionIsEmpty()
    {
        // Arrange
        await SeedAsync(new TestItem { Name = "A" });

        // Act
        await _repository.DeleteAsync(new List<object>(), TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.Items.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task DetachStopsTrackingEntity()
    {
        // Arrange
        var item = new TestItem { Name = "A" };
        await SeedAsync(item);
        var tracked = await _repository.GetByIdAsync(item.Id, TestContext.Current.CancellationToken);
        _context.Entry(tracked!).State.ShouldBe(EntityState.Unchanged);

        // Act
        _repository.Detach(tracked!);

        // Assert
        _context.Entry(tracked!).State.ShouldBe(EntityState.Detached);
    }

    [Fact]
    public async Task DetachPreventsChangesFromBeingPersisted()
    {
        // Arrange
        var item = new TestItem { Name = "A" };
        await SeedAsync(item);
        var tracked = await _repository.GetByIdAsync(item.Id, TestContext.Current.CancellationToken);
        tracked!.Name = "Changed";

        // Act
        _repository.Detach(tracked);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var stored = await _context.Items.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        stored.Name.ShouldBe("A");
    }

    private sealed class TestItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestItem> Items => Set<TestItem>();
    }
}
