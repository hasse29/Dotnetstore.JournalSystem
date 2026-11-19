using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Microsoft.EntityFrameworkCore;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Contact.Tests.AddressTypes;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Services;

public sealed class ContactUnitOfWorkTests : IDisposable
{
    private readonly DbContextOptions<ContactDataContext> _options;
    private readonly ContactDataContext _context;
    private readonly AddressTypeRepository _repository;
    private readonly ContactUnitOfWork _unitOfWork;

    public ContactUnitOfWorkTests(SqlServerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(_options);
        _context.Database.EnsureCreated();
        _repository = new AddressTypeRepository(_context);
        _unitOfWork = new ContactUnitOfWork(_context, _repository);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
        using var cleanup = new ContactDataContext(_options);
        cleanup.Database.EnsureDeleted();
    }

    private static AddressType CreateAddressType()
    {
        return AddressTypeTestData.Create(Guid.NewGuid(), "Home");
    }

    private async Task<int> CountStoredAsync()
    {
        return await _context.AddressTypes.AsNoTracking().CountAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void AddressTypesReturnsTheInjectedRepository()
    {
        // Arrange
        IContactUnitOfWork unitOfWork = _unitOfWork;

        // Act
        var result = unitOfWork.AddressTypes;

        // Assert
        result.ShouldBeSameAs(_repository);
    }

    [Fact]
    public async Task SaveChangesAsyncPersistsAndReturnsNumberOfChanges()
    {
        // Arrange
        IContactUnitOfWork unitOfWork = _unitOfWork;
        await unitOfWork.AddressTypes.AddAsync(CreateAddressType(), TestContext.Current.CancellationToken);

        // Act
        var changes = await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        changes.ShouldBe(1);
        (await CountStoredAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task SaveChangesAsyncReturnsZeroWhenNothingChanged()
    {
        // Arrange
        IContactUnitOfWork unitOfWork = _unitOfWork;

        // Act
        var changes = await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        changes.ShouldBe(0);
    }

    [Fact]
    public async Task SaveChangesAsyncThrowsOriginalExceptionTypeOnFailure()
    {
        // Arrange
        IContactUnitOfWork unitOfWork = _unitOfWork;
        var addressType = CreateAddressType();
        await unitOfWork.AddressTypes.AddAsync(addressType, TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
        await unitOfWork.AddressTypes.AddAsync(
            AddressTypeTestData.Create(addressType.Id, "Duplicate"),
            TestContext.Current.CancellationToken);

        // Act
        var act = async () => await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<DbUpdateException>(act);
    }

    [Fact]
    public async Task CommitAsyncKeepsChangesMadeInsideTransaction()
    {
        // Arrange
        IContactUnitOfWork unitOfWork = _unitOfWork;
        unitOfWork.CreateTransaction();
        await unitOfWork.AddressTypes.AddAsync(CreateAddressType(), TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        // Assert
        (await CountStoredAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task RollbackAsyncDiscardsChangesMadeInsideTransaction()
    {
        // Arrange
        IContactUnitOfWork unitOfWork = _unitOfWork;
        unitOfWork.CreateTransaction();
        await unitOfWork.AddressTypes.AddAsync(CreateAddressType(), TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        await unitOfWork.RollbackAsync(TestContext.Current.CancellationToken);

        // Assert
        (await CountStoredAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task CommitAsyncThrowsWhenNoTransactionWasCreated()
    {
        // Arrange
        IContactUnitOfWork unitOfWork = _unitOfWork;

        // Act
        var act = async () => await unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task RollbackAsyncThrowsWhenNoTransactionWasCreated()
    {
        // Arrange
        IContactUnitOfWork unitOfWork = _unitOfWork;

        // Act
        var act = async () => await unitOfWork.RollbackAsync(TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
    }

    [Fact]
    public void DisposeDisposesTheDataContext()
    {
        // Arrange

        // Act
        _unitOfWork.Dispose();

        // Assert
        Should.Throw<ObjectDisposedException>(() => _context.AddressTypes.ToList());
    }

    [Fact]
    public void DisposeCanBeCalledMoreThanOnce()
    {
        // Arrange
        _unitOfWork.Dispose();

        // Act
        var act = () => _unitOfWork.Dispose();

        // Assert
        Should.NotThrow(act);
    }
}
