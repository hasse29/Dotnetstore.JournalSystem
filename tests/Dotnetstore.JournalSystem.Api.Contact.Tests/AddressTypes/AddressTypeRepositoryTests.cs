using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.AddressTypes;

public sealed class AddressTypeRepositoryTests : IDisposable
{
    private readonly ContactDataContext _context;
    private readonly AddressTypeRepository _repository;

    public AddressTypeRepositoryTests(SqlServerFixture fixture)
    {
        var options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(options);
        _context.Database.EnsureCreated();
        _repository = new AddressTypeRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static AddressType CreateAddressType(string name = "Administrator")
    {
        return AddressTypeTestData.Create(Guid.NewGuid(), name, "Description");
    }

    private async Task SeedAsync(params AddressType[] addressTypes)
    {
        _context.AddressTypes.AddRange(addressTypes);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public void RepositoryIsAGenericRepositoryOfAddressType()
    {
        // Arrange

        // Act
        var isGenericRepository = _repository is IGenericRepository<AddressType>;

        // Assert
        isGenericRepository.ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsyncPersistsAddressTypeWithAllValues()
    {
        // Arrange
        var addressType = CreateAddressType();

        // Act
        await _repository.AddAsync(addressType, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.AddressTypes.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(addressType.Id);
        stored.Name.ShouldBe("Administrator");
        stored.Description.ShouldBe("Description");
    }

    [Fact]
    public async Task AddAsyncDoesNotPersistWhenChangesAreNotSaved()
    {
        // Arrange
        var addressType = CreateAddressType();

        // Act
        await _repository.AddAsync(addressType, TestContext.Current.CancellationToken);

        // Assert
        (await _context.AddressTypes.AsNoTracking().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsAddressTypeWhenItExists()
    {
        // Arrange
        var addressType = CreateAddressType();
        await SeedAsync(addressType);

        // Act
        var result = await _repository.GetByIdAsync(addressType.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(addressType.Name);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateAddressType());

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllAddressTypes()
    {
        // Arrange
        await SeedAsync(CreateAddressType("A"), CreateAddressType("B"));

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Name).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoAddressTypesExist()
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
        var addressType = CreateAddressType();
        await SeedAsync(addressType);
        var changed = AddressTypeTestData.Create(addressType.Id, "Renamed", "Changed");

        // Act
        _repository.Update(changed);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.AddressTypes.SingleAsync(TestContext.Current.CancellationToken);
        stored.Name.ShouldBe("Renamed");
        stored.Description.ShouldBe("Changed");
    }

    [Fact]
    public async Task DeleteAsyncRemovesAddressTypeWhenItExists()
    {
        // Arrange
        var addressType = CreateAddressType();
        await SeedAsync(addressType);

        // Act
        await _repository.DeleteAsync(addressType.Id, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.AddressTypes.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteAsyncKeepsAddressTypesWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateAddressType());

        // Act
        await _repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.AddressTypes.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }
}
