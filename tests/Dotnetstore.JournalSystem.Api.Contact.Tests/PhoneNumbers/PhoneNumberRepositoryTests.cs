using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;

public sealed class PhoneNumberRepositoryTests : IDisposable
{
    private readonly ContactDataContext _context;
    private readonly PhoneNumberRepository _repository;

    public PhoneNumberRepositoryTests(SqlServerFixture fixture)
    {
        var options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(options);
        _context.Database.EnsureCreated();
        _repository = new PhoneNumberRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static PhoneNumber CreatePhoneNumber(string number = "+46 70 123 45 67")
    {
        return PhoneNumberTestData.Create(Guid.NewGuid(), number);
    }

    private async Task SeedAsync(params PhoneNumber[] phoneNumberes)
    {
        _context.PhoneNumbers.AddRange(phoneNumberes);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public void RepositoryIsAGenericRepositoryOfPhoneNumber()
    {
        // Arrange

        // Act
        var isGenericRepository = _repository is IGenericRepository<PhoneNumber>;

        // Assert
        isGenericRepository.ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsyncPersistsPhoneNumberWithAllValues()
    {
        // Arrange
        var phoneNumber = CreatePhoneNumber();

        // Act
        await _repository.AddAsync(phoneNumber, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.PhoneNumbers.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(phoneNumber.Id);
        stored.PhoneNumberTypeId.ShouldBe(PhoneNumberTestData.PhoneNumberTypeId);
        stored.ObjectId.ShouldBe(PhoneNumberTestData.ObjectId);
        stored.Number.ShouldBe("+46 70 123 45 67");
    }

    [Fact]
    public async Task AddAsyncDoesNotPersistWhenChangesAreNotSaved()
    {
        // Arrange
        var phoneNumber = CreatePhoneNumber();

        // Act
        await _repository.AddAsync(phoneNumber, TestContext.Current.CancellationToken);

        // Assert
        (await _context.PhoneNumbers.AsNoTracking().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsPhoneNumberWhenItExists()
    {
        // Arrange
        var phoneNumber = CreatePhoneNumber();
        await SeedAsync(phoneNumber);

        // Act
        var result = await _repository.GetByIdAsync(phoneNumber.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Number.ShouldBe(phoneNumber.Number);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreatePhoneNumber());

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllPhoneNumbers()
    {
        // Arrange
        await SeedAsync(CreatePhoneNumber("+46 70 000 00 01"), CreatePhoneNumber("+46 70 000 00 02"));

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Number).ShouldBe(["+46 70 000 00 01", "+46 70 000 00 02"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoPhoneNumbersExist()
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
        var phoneNumber = CreatePhoneNumber();
        await SeedAsync(phoneNumber);
        var changed = PhoneNumberTestData.Create(phoneNumber.Id, "+46 70 999 99 99");

        // Act
        _repository.Update(changed);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.PhoneNumbers.SingleAsync(TestContext.Current.CancellationToken);
        stored.Number.ShouldBe("+46 70 999 99 99");
    }

    [Fact]
    public async Task DeleteAsyncRemovesPhoneNumberWhenItExists()
    {
        // Arrange
        var phoneNumber = CreatePhoneNumber();
        await SeedAsync(phoneNumber);

        // Act
        await _repository.DeleteAsync(phoneNumber.Id, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.PhoneNumbers.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteAsyncKeepsPhoneNumbersWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreatePhoneNumber());

        // Act
        await _repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.PhoneNumbers.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task AnyWithPhoneNumberTypeAsyncReturnsTrueWhenAnPhoneNumberUsesTheType()
    {
        // Arrange
        IPhoneNumberRepository repository = _repository;
        await SeedAsync(CreatePhoneNumber());

        // Act
        var result = await repository.AnyWithPhoneNumberTypeAsync(PhoneNumberTestData.PhoneNumberTypeId, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task AnyWithPhoneNumberTypeAsyncReturnsFalseForATypeNoPhoneNumberUses()
    {
        // Arrange
        IPhoneNumberRepository repository = _repository;
        await SeedAsync(CreatePhoneNumber());

        // Act
        var result = await repository.AnyWithPhoneNumberTypeAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task AnyWithPhoneNumberTypeAsyncReturnsFalseWhenThereAreNoPhoneNumbers()
    {
        // Arrange
        IPhoneNumberRepository repository = _repository;

        // Act
        var result = await repository.AnyWithPhoneNumberTypeAsync(PhoneNumberTestData.PhoneNumberTypeId, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByObjectIdAsyncReturnsOnlyTheRowsOfThatObject()
    {
        // Arrange
        IPhoneNumberRepository repository = _repository;
        var wanted = Guid.NewGuid();
        await SeedAsync(
            PhoneNumberTestData.Create(Guid.NewGuid(), "+46 70 111 11 11", objectId: wanted),
            PhoneNumberTestData.Create(Guid.NewGuid(), "+46 70 222 22 22", objectId: Guid.NewGuid()));

        // Act
        var result = await repository.GetByObjectIdAsync(wanted, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldHaveSingleItem().Number.ShouldBe("+46 70 111 11 11");
        _context.ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByObjectIdAsyncReturnsEmptyForAnObjectWithoutRows()
    {
        // Arrange
        IPhoneNumberRepository repository = _repository;
        await SeedAsync(PhoneNumberTestData.Create(Guid.NewGuid(), "+46 70 111 11 11", objectId: Guid.NewGuid()));

        // Act
        var result = await repository.GetByObjectIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEmpty();
    }
}
