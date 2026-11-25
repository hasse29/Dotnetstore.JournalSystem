using Dotnetstore.Security.Handlers;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;

public sealed class EmailAddressServiceTests : IDisposable
{
    private readonly DbContextOptions<ContactDataContext> _options;
    private readonly ContactDataContext _context;
    private readonly ContactUnitOfWork _unitOfWork;
    private readonly EmailAddressService _service;
    private readonly IEncryptionHandler _encryption = TestEncryption.Create();
    private readonly Guid _emailAddressTypeId = Guid.NewGuid();

    public EmailAddressServiceTests(SqlServerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(_options);
        _context.Database.EnsureCreated();
        _context.EmailAddressTypes.Add(EmailAddressTypeTestData.Create(_emailAddressTypeId, "Work"));
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        _unitOfWork = new ContactUnitOfWork(
            _context,
            new AddressTypeRepository(_context),
            new AddressRepository(_context),
            new EmailAddressTypeRepository(_context),
            new EmailAddressRepository(_context),
            new PhoneNumberTypeRepository(_context),
            new PhoneNumberRepository(_context));
        _service = new EmailAddressService(_unitOfWork, _encryption);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
        using var cleanup = new ContactDataContext(_options);
        cleanup.Database.EnsureDeleted();
    }

    private EditEmailAddressRequest CreateRequest(string address = "anna@example.com", Guid? emailAddressTypeId = null)
    {
        return new EditEmailAddressRequest(emailAddressTypeId ?? _emailAddressTypeId, EmailAddressTestData.ObjectId, address);
    }

    private async Task<Guid> SeedAsync(string address = "seeded@example.com")
    {
        IEmailAddressService service = _service;
        var result = await service.CreateAsync(CreateRequest(address), null, TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
        return result.Value.Id;
    }

    private async Task<int> CountStoredAsync()
    {
        return await _context.EmailAddresses.AsNoTracking().CountAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllEmailAddressesAsResponses()
    {
        // Arrange
        IEmailAddressService service = _service;
        await SeedAsync("a@example.com");
        await SeedAsync("b@example.com");

        // Act
        var result = await service.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Address).ShouldBe(["a@example.com", "b@example.com"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoEmailAddressesExist()
    {
        // Arrange
        IEmailAddressService service = _service;

        // Act
        var result = await service.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsyncReturnsEmailAddressAsResponse()
    {
        // Arrange
        IEmailAddressService service = _service;
        var id = await SeedAsync("find.me@example.com");

        // Act
        var result = await service.GetByIdAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(id);
        result.Value.Address.ShouldBe("find.me@example.com");
        result.Value.EmailAddressTypeId.ShouldBe(_emailAddressTypeId);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNotFoundForUnknownId()
    {
        // Arrange
        IEmailAddressService service = _service;
        await SeedAsync();

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task CreateAsyncPersistsEmailAddressAndReturnsResponse()
    {
        // Arrange
        IEmailAddressService service = _service;
        var userId = Guid.NewGuid();

        // Act
        var result = await service.CreateAsync(CreateRequest(), userId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Address.ShouldBe("anna@example.com");
        result.Value.Id.ShouldNotBe(Guid.Empty);
        _context.ChangeTracker.Clear();
        var stored = await _context.EmailAddresses.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(result.Value.Id);
        stored.CreatedBy.ShouldBe(userId);
        stored.Address.ShouldNotContain("anna");
        stored.Address.ShouldStartWith("dse1.");
        stored.Decrypt(_encryption).Address.ShouldBe("anna@example.com");
    }

    [Fact]
    public async Task CreateAsyncReturnsInvalidAndPersistsNothingForUnknownEmailAddressType()
    {
        // Arrange
        IEmailAddressService service = _service;

        // Act
        var result = await service.CreateAsync(CreateRequest(emailAddressTypeId: Guid.NewGuid()), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldHaveSingleItem().Identifier.ShouldBe("EmailAddressTypeId");
        (await CountStoredAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task CreateAsyncThrowsAndPersistsNothingForInvalidRequest()
    {
        // Arrange
        IEmailAddressService service = _service;

        // Act
        var act = async () => await service.CreateAsync(CreateRequest("not-an-email"), null, TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
        (await CountStoredAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task UpdateAsyncPersistsChangesAndReturnsResponse()
    {
        // Arrange
        IEmailAddressService service = _service;
        var id = await SeedAsync();
        var editor = Guid.NewGuid();

        // Act
        var result = await service.UpdateAsync(id, CreateRequest("changed@example.com"), editor, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Address.ShouldBe("changed@example.com");
        _context.ChangeTracker.Clear();
        var stored = await _context.EmailAddresses.SingleAsync(TestContext.Current.CancellationToken);
        stored.Address.ShouldNotContain("changed");
        stored.Decrypt(_encryption).Address.ShouldBe("changed@example.com");
        stored.LastUpdatedBy.ShouldBe(editor);
        stored.LastUpdatedDate.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateAsyncReturnsNotFoundForUnknownId()
    {
        // Arrange
        IEmailAddressService service = _service;

        // Act
        var result = await service.UpdateAsync(Guid.NewGuid(), CreateRequest(), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task UpdateAsyncReturnsInvalidAndKeepsStoredValuesForUnknownEmailAddressType()
    {
        // Arrange
        IEmailAddressService service = _service;
        var id = await SeedAsync("original@example.com");

        // Act
        var result = await service.UpdateAsync(id, CreateRequest("changed@example.com", Guid.NewGuid()), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Invalid);
        _context.ChangeTracker.Clear();
        (await _context.EmailAddresses.SingleAsync(TestContext.Current.CancellationToken)).Decrypt(_encryption).Address.ShouldBe("original@example.com");
    }

    [Fact]
    public async Task UpdateAsyncKeepsStoredValuesWhenRequestIsInvalid()
    {
        // Arrange
        IEmailAddressService service = _service;
        var id = await SeedAsync("original@example.com");

        // Act
        var act = async () => await service.UpdateAsync(id, CreateRequest("not-an-email"), null, TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
        _context.ChangeTracker.Clear();
        (await _context.EmailAddresses.SingleAsync(TestContext.Current.CancellationToken)).Decrypt(_encryption).Address.ShouldBe("original@example.com");
    }

    [Fact]
    public async Task DeleteAsyncRemovesExistingEmailAddress()
    {
        // Arrange
        IEmailAddressService service = _service;
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
        IEmailAddressService service = _service;
        await SeedAsync();

        // Act
        var result = await service.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
        (await CountStoredAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task GetByObjectIdAsyncReturnsOnlyThatObjectsRowsDecrypted()
    {
        // Arrange
        IEmailAddressService service = _service;
        var wanted = Guid.NewGuid();
        await service.CreateAsync(CreateRequest("wanted@example.com") with { ObjectId = wanted }, null, TestContext.Current.CancellationToken);
        await service.CreateAsync(CreateRequest("other@example.com") with { ObjectId = Guid.NewGuid() }, null, TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();

        // Act
        var result = await service.GetByObjectIdAsync(wanted, TestContext.Current.CancellationToken);

        // Assert
        var item = result.ShouldHaveSingleItem();
        item.ObjectId.ShouldBe(wanted);
        item.Address.ShouldBe("wanted@example.com");
    }

    [Fact]
    public async Task GetByObjectIdAsyncReturnsEmptyForAnObjectWithoutRows()
    {
        // Arrange
        IEmailAddressService service = _service;
        await service.CreateAsync(CreateRequest("wanted@example.com") with { ObjectId = Guid.NewGuid() }, null, TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();

        // Act
        var result = await service.GetByObjectIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEmpty();
    }
}
