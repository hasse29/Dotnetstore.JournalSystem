using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;

public sealed class EmailAddressRepositoryTests : IDisposable
{
    private readonly ContactDataContext _context;
    private readonly EmailAddressRepository _repository;

    public EmailAddressRepositoryTests(SqlServerFixture fixture)
    {
        var options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(options);
        _context.Database.EnsureCreated();
        _repository = new EmailAddressRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static EmailAddress CreateEmailAddress(string address = "anna@example.com")
    {
        return EmailAddressTestData.Create(Guid.NewGuid(), address);
    }

    private async Task SeedAsync(params EmailAddress[] emailAddresses)
    {
        _context.EmailAddresses.AddRange(emailAddresses);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public void RepositoryIsAGenericRepositoryOfEmailAddress()
    {
        // Arrange

        // Act
        var isGenericRepository = _repository is IGenericRepository<EmailAddress>;

        // Assert
        isGenericRepository.ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsyncPersistsEmailAddressWithAllValues()
    {
        // Arrange
        var emailAddress = CreateEmailAddress();

        // Act
        await _repository.AddAsync(emailAddress, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.EmailAddresses.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(emailAddress.Id);
        stored.EmailAddressTypeId.ShouldBe(EmailAddressTestData.EmailAddressTypeId);
        stored.ObjectId.ShouldBe(EmailAddressTestData.ObjectId);
        stored.Address.ShouldBe("anna@example.com");
    }

    [Fact]
    public async Task AddAsyncDoesNotPersistWhenChangesAreNotSaved()
    {
        // Arrange
        var emailAddress = CreateEmailAddress();

        // Act
        await _repository.AddAsync(emailAddress, TestContext.Current.CancellationToken);

        // Assert
        (await _context.EmailAddresses.AsNoTracking().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsEmailAddressWhenItExists()
    {
        // Arrange
        var emailAddress = CreateEmailAddress();
        await SeedAsync(emailAddress);

        // Act
        var result = await _repository.GetByIdAsync(emailAddress.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Address.ShouldBe(emailAddress.Address);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateEmailAddress());

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllEmailAddresses()
    {
        // Arrange
        await SeedAsync(CreateEmailAddress("a@example.com"), CreateEmailAddress("b@example.com"));

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Address).ShouldBe(["a@example.com", "b@example.com"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoEmailAddressesExist()
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
        var emailAddress = CreateEmailAddress();
        await SeedAsync(emailAddress);
        var changed = EmailAddressTestData.Create(emailAddress.Id, "changed@example.com");

        // Act
        _repository.Update(changed);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.EmailAddresses.SingleAsync(TestContext.Current.CancellationToken);
        stored.Address.ShouldBe("changed@example.com");
    }

    [Fact]
    public async Task DeleteAsyncRemovesEmailAddressWhenItExists()
    {
        // Arrange
        var emailAddress = CreateEmailAddress();
        await SeedAsync(emailAddress);

        // Act
        await _repository.DeleteAsync(emailAddress.Id, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.EmailAddresses.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteAsyncKeepsEmailAddressesWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateEmailAddress());

        // Act
        await _repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.EmailAddresses.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task AnyWithEmailAddressTypeAsyncReturnsTrueWhenAnEmailAddressUsesTheType()
    {
        // Arrange
        IEmailAddressRepository repository = _repository;
        await SeedAsync(CreateEmailAddress());

        // Act
        var result = await repository.AnyWithEmailAddressTypeAsync(EmailAddressTestData.EmailAddressTypeId, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task AnyWithEmailAddressTypeAsyncReturnsFalseForATypeNoEmailAddressUses()
    {
        // Arrange
        IEmailAddressRepository repository = _repository;
        await SeedAsync(CreateEmailAddress());

        // Act
        var result = await repository.AnyWithEmailAddressTypeAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task AnyWithEmailAddressTypeAsyncReturnsFalseWhenThereAreNoEmailAddresses()
    {
        // Arrange
        IEmailAddressRepository repository = _repository;

        // Act
        var result = await repository.AnyWithEmailAddressTypeAsync(EmailAddressTestData.EmailAddressTypeId, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeFalse();
    }
}
