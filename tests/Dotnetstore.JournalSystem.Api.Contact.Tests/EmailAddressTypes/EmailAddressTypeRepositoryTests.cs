using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddressTypes;

public sealed class EmailAddressTypeRepositoryTests : IDisposable
{
    private readonly ContactDataContext _context;
    private readonly EmailAddressTypeRepository _repository;

    public EmailAddressTypeRepositoryTests(SqlServerFixture fixture)
    {
        var options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(options);
        _context.Database.EnsureCreated();
        _repository = new EmailAddressTypeRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static EmailAddressType CreateEmailAddressType(string name = "Administrator")
    {
        return EmailAddressTypeTestData.Create(Guid.NewGuid(), name, "Description");
    }

    private async Task SeedAsync(params EmailAddressType[] emailAddressTypes)
    {
        _context.EmailAddressTypes.AddRange(emailAddressTypes);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public void RepositoryIsAGenericRepositoryOfEmailAddressType()
    {
        // Arrange

        // Act
        var isGenericRepository = _repository is IGenericRepository<EmailAddressType>;

        // Assert
        isGenericRepository.ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsyncPersistsEmailAddressTypeWithAllValues()
    {
        // Arrange
        var emailAddressType = CreateEmailAddressType();

        // Act
        await _repository.AddAsync(emailAddressType, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.EmailAddressTypes.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(emailAddressType.Id);
        stored.Name.ShouldBe("Administrator");
        stored.Description.ShouldBe("Description");
    }

    [Fact]
    public async Task AddAsyncDoesNotPersistWhenChangesAreNotSaved()
    {
        // Arrange
        var emailAddressType = CreateEmailAddressType();

        // Act
        await _repository.AddAsync(emailAddressType, TestContext.Current.CancellationToken);

        // Assert
        (await _context.EmailAddressTypes.AsNoTracking().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsEmailAddressTypeWhenItExists()
    {
        // Arrange
        var emailAddressType = CreateEmailAddressType();
        await SeedAsync(emailAddressType);

        // Act
        var result = await _repository.GetByIdAsync(emailAddressType.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(emailAddressType.Name);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateEmailAddressType());

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllEmailAddressTypes()
    {
        // Arrange
        await SeedAsync(CreateEmailAddressType("A"), CreateEmailAddressType("B"));

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Name).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoEmailAddressTypesExist()
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
        var emailAddressType = CreateEmailAddressType();
        await SeedAsync(emailAddressType);
        var changed = EmailAddressTypeTestData.Create(emailAddressType.Id, "Renamed", "Changed");

        // Act
        _repository.Update(changed);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.EmailAddressTypes.SingleAsync(TestContext.Current.CancellationToken);
        stored.Name.ShouldBe("Renamed");
        stored.Description.ShouldBe("Changed");
    }

    [Fact]
    public async Task DeleteAsyncRemovesEmailAddressTypeWhenItExists()
    {
        // Arrange
        var emailAddressType = CreateEmailAddressType();
        await SeedAsync(emailAddressType);

        // Act
        await _repository.DeleteAsync(emailAddressType.Id, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.EmailAddressTypes.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteAsyncKeepsEmailAddressTypesWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateEmailAddressType());

        // Act
        await _repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.EmailAddressTypes.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }
}
