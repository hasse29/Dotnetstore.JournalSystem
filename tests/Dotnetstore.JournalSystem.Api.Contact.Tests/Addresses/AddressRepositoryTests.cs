using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Addresses;

public sealed class AddressRepositoryTests : IDisposable
{
    private readonly ContactDataContext _context;
    private readonly AddressRepository _repository;

    public AddressRepositoryTests(SqlServerFixture fixture)
    {
        var options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(options);
        _context.Database.EnsureCreated();
        _repository = new AddressRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static Address CreateAddress(string street1 = "Main Street 1")
    {
        return AddressTestData.Create(Guid.NewGuid(), street1, "Floor 2", co: "c/o Anna");
    }

    private async Task SeedAsync(params Address[] addresses)
    {
        _context.Addresses.AddRange(addresses);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public void RepositoryIsAGenericRepositoryOfAddress()
    {
        // Arrange

        // Act
        var isGenericRepository = _repository is IGenericRepository<Address>;

        // Assert
        isGenericRepository.ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsyncPersistsAddressWithAllValues()
    {
        // Arrange
        var address = CreateAddress();

        // Act
        await _repository.AddAsync(address, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.Addresses.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(address.Id);
        stored.AddressTypeId.ShouldBe(AddressTestData.AddressTypeId);
        stored.ObjectId.ShouldBe(AddressTestData.ObjectId);
        stored.CO.ShouldBe("c/o Anna");
        stored.Street1.ShouldBe("Main Street 1");
        stored.Street2.ShouldBe("Floor 2");
        stored.PostalCode.ShouldBe("12345");
        stored.City.ShouldBe("Stockholm");
        stored.State.ShouldBe("Stockholm");
        stored.Country.ShouldBe("Sweden");
    }

    [Fact]
    public async Task AddAsyncDoesNotPersistWhenChangesAreNotSaved()
    {
        // Arrange
        var address = CreateAddress();

        // Act
        await _repository.AddAsync(address, TestContext.Current.CancellationToken);

        // Assert
        (await _context.Addresses.AsNoTracking().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsAddressWhenItExists()
    {
        // Arrange
        var address = CreateAddress();
        await SeedAsync(address);

        // Act
        var result = await _repository.GetByIdAsync(address.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Street1.ShouldBe(address.Street1);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateAddress());

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllAddresses()
    {
        // Arrange
        await SeedAsync(CreateAddress("A"), CreateAddress("B"));

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Street1).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoAddressesExist()
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
        var address = CreateAddress();
        await SeedAsync(address);
        var changed = AddressTestData.Create(address.Id, "Renamed Street", null, "99999", "Gothenburg");

        // Act
        _repository.Update(changed);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.Addresses.SingleAsync(TestContext.Current.CancellationToken);
        stored.Street1.ShouldBe("Renamed Street");
        stored.CO.ShouldBeNull();
        stored.Street2.ShouldBeNull();
        stored.City.ShouldBe("Gothenburg");
    }

    [Fact]
    public async Task DeleteAsyncRemovesAddressWhenItExists()
    {
        // Arrange
        var address = CreateAddress();
        await SeedAsync(address);

        // Act
        await _repository.DeleteAsync(address.Id, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.Addresses.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteAsyncKeepsAddressesWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateAddress());

        // Act
        await _repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.Addresses.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task AnyWithAddressTypeAsyncReturnsTrueWhenAnAddressUsesTheType()
    {
        // Arrange
        IAddressRepository repository = _repository;
        await SeedAsync(CreateAddress());

        // Act
        var result = await repository.AnyWithAddressTypeAsync(AddressTestData.AddressTypeId, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task AnyWithAddressTypeAsyncReturnsFalseForATypeNoAddressUses()
    {
        // Arrange
        IAddressRepository repository = _repository;
        await SeedAsync(CreateAddress());

        // Act
        var result = await repository.AnyWithAddressTypeAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task AnyWithAddressTypeAsyncReturnsFalseWhenThereAreNoAddresses()
    {
        // Arrange
        IAddressRepository repository = _repository;

        // Act
        var result = await repository.AnyWithAddressTypeAsync(AddressTestData.AddressTypeId, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeFalse();
    }
}
