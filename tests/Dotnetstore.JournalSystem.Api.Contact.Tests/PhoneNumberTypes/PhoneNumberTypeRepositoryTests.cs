using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumberTypes;

public sealed class PhoneNumberTypeRepositoryTests : IDisposable
{
    private readonly ContactDataContext _context;
    private readonly PhoneNumberTypeRepository _repository;

    public PhoneNumberTypeRepositoryTests(SqlServerFixture fixture)
    {
        var options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(options);
        _context.Database.EnsureCreated();
        _repository = new PhoneNumberTypeRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static PhoneNumberType CreatePhoneNumberType(string name = "Administrator")
    {
        return PhoneNumberTypeTestData.Create(Guid.NewGuid(), name, "Description");
    }

    private async Task SeedAsync(params PhoneNumberType[] phoneNumberTypes)
    {
        _context.PhoneNumberTypes.AddRange(phoneNumberTypes);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public void RepositoryIsAGenericRepositoryOfPhoneNumberType()
    {
        // Arrange

        // Act
        var isGenericRepository = _repository is IGenericRepository<PhoneNumberType>;

        // Assert
        isGenericRepository.ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsyncPersistsPhoneNumberTypeWithAllValues()
    {
        // Arrange
        var phoneNumberType = CreatePhoneNumberType();

        // Act
        await _repository.AddAsync(phoneNumberType, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.PhoneNumberTypes.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(phoneNumberType.Id);
        stored.Name.ShouldBe("Administrator");
        stored.Description.ShouldBe("Description");
    }

    [Fact]
    public async Task AddAsyncDoesNotPersistWhenChangesAreNotSaved()
    {
        // Arrange
        var phoneNumberType = CreatePhoneNumberType();

        // Act
        await _repository.AddAsync(phoneNumberType, TestContext.Current.CancellationToken);

        // Assert
        (await _context.PhoneNumberTypes.AsNoTracking().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsPhoneNumberTypeWhenItExists()
    {
        // Arrange
        var phoneNumberType = CreatePhoneNumberType();
        await SeedAsync(phoneNumberType);

        // Act
        var result = await _repository.GetByIdAsync(phoneNumberType.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(phoneNumberType.Name);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreatePhoneNumberType());

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllPhoneNumberTypes()
    {
        // Arrange
        await SeedAsync(CreatePhoneNumberType("A"), CreatePhoneNumberType("B"));

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Name).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoPhoneNumberTypesExist()
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
        var phoneNumberType = CreatePhoneNumberType();
        await SeedAsync(phoneNumberType);
        var changed = PhoneNumberTypeTestData.Create(phoneNumberType.Id, "Renamed", "Changed");

        // Act
        _repository.Update(changed);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.PhoneNumberTypes.SingleAsync(TestContext.Current.CancellationToken);
        stored.Name.ShouldBe("Renamed");
        stored.Description.ShouldBe("Changed");
    }

    [Fact]
    public async Task DeleteAsyncRemovesPhoneNumberTypeWhenItExists()
    {
        // Arrange
        var phoneNumberType = CreatePhoneNumberType();
        await SeedAsync(phoneNumberType);

        // Act
        await _repository.DeleteAsync(phoneNumberType.Id, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.PhoneNumberTypes.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteAsyncKeepsPhoneNumberTypesWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreatePhoneNumberType());

        // Act
        await _repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.PhoneNumberTypes.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }
}
