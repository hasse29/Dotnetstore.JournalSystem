using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumberTypes;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;

public sealed class PhoneNumberServiceTests : IDisposable
{
    private readonly DbContextOptions<ContactDataContext> _options;
    private readonly ContactDataContext _context;
    private readonly ContactUnitOfWork _unitOfWork;
    private readonly PhoneNumberService _service;
    private readonly Guid _phoneNumberTypeId = Guid.NewGuid();

    public PhoneNumberServiceTests(SqlServerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(_options);
        _context.Database.EnsureCreated();
        _context.PhoneNumberTypes.Add(PhoneNumberTypeTestData.Create(_phoneNumberTypeId, "Work"));
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
        _service = new PhoneNumberService(_unitOfWork);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
        using var cleanup = new ContactDataContext(_options);
        cleanup.Database.EnsureDeleted();
    }

    private EditPhoneNumberRequest CreateRequest(string number = "+46 70 123 45 67", Guid? phoneNumberTypeId = null)
    {
        return new EditPhoneNumberRequest(phoneNumberTypeId ?? _phoneNumberTypeId, PhoneNumberTestData.ObjectId, number);
    }

    private async Task<Guid> SeedAsync(string number = "+46 70 777 77 77")
    {
        IPhoneNumberService service = _service;
        var result = await service.CreateAsync(CreateRequest(number), null, TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
        return result.Value.Id;
    }

    private async Task<int> CountStoredAsync()
    {
        return await _context.PhoneNumbers.AsNoTracking().CountAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllPhoneNumbersAsResponses()
    {
        // Arrange
        IPhoneNumberService service = _service;
        await SeedAsync("+46 70 000 00 01");
        await SeedAsync("+46 70 000 00 02");

        // Act
        var result = await service.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Number).ShouldBe(["+46 70 000 00 01", "+46 70 000 00 02"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoPhoneNumbersExist()
    {
        // Arrange
        IPhoneNumberService service = _service;

        // Act
        var result = await service.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsyncReturnsPhoneNumberAsResponse()
    {
        // Arrange
        IPhoneNumberService service = _service;
        var id = await SeedAsync("+46 70 555 55 55");

        // Act
        var result = await service.GetByIdAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(id);
        result.Value.Number.ShouldBe("+46 70 555 55 55");
        result.Value.PhoneNumberTypeId.ShouldBe(_phoneNumberTypeId);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNotFoundForUnknownId()
    {
        // Arrange
        IPhoneNumberService service = _service;
        await SeedAsync();

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task CreateAsyncPersistsPhoneNumberAndReturnsResponse()
    {
        // Arrange
        IPhoneNumberService service = _service;
        var userId = Guid.NewGuid();

        // Act
        var result = await service.CreateAsync(CreateRequest(), userId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.ShouldBe("+46 70 123 45 67");
        result.Value.Id.ShouldNotBe(Guid.Empty);
        _context.ChangeTracker.Clear();
        var stored = await _context.PhoneNumbers.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(result.Value.Id);
        stored.CreatedBy.ShouldBe(userId);
    }

    [Fact]
    public async Task CreateAsyncReturnsInvalidAndPersistsNothingForUnknownPhoneNumberType()
    {
        // Arrange
        IPhoneNumberService service = _service;

        // Act
        var result = await service.CreateAsync(CreateRequest(phoneNumberTypeId: Guid.NewGuid()), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldHaveSingleItem().Identifier.ShouldBe("PhoneNumberTypeId");
        (await CountStoredAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task CreateAsyncThrowsAndPersistsNothingForInvalidRequest()
    {
        // Arrange
        IPhoneNumberService service = _service;

        // Act
        var act = async () => await service.CreateAsync(CreateRequest("not-a-number"), null, TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
        (await CountStoredAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task UpdateAsyncPersistsChangesAndReturnsResponse()
    {
        // Arrange
        IPhoneNumberService service = _service;
        var id = await SeedAsync();
        var editor = Guid.NewGuid();

        // Act
        var result = await service.UpdateAsync(id, CreateRequest("+46 70 999 99 99"), editor, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Number.ShouldBe("+46 70 999 99 99");
        _context.ChangeTracker.Clear();
        var stored = await _context.PhoneNumbers.SingleAsync(TestContext.Current.CancellationToken);
        stored.Number.ShouldBe("+46 70 999 99 99");
        stored.LastUpdatedBy.ShouldBe(editor);
        stored.LastUpdatedDate.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateAsyncReturnsNotFoundForUnknownId()
    {
        // Arrange
        IPhoneNumberService service = _service;

        // Act
        var result = await service.UpdateAsync(Guid.NewGuid(), CreateRequest(), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task UpdateAsyncReturnsInvalidAndKeepsStoredValuesForUnknownPhoneNumberType()
    {
        // Arrange
        IPhoneNumberService service = _service;
        var id = await SeedAsync("+46 70 444 44 44");

        // Act
        var result = await service.UpdateAsync(id, CreateRequest("+46 70 999 99 99", Guid.NewGuid()), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Invalid);
        _context.ChangeTracker.Clear();
        (await _context.PhoneNumbers.SingleAsync(TestContext.Current.CancellationToken)).Number.ShouldBe("+46 70 444 44 44");
    }

    [Fact]
    public async Task UpdateAsyncKeepsStoredValuesWhenRequestIsInvalid()
    {
        // Arrange
        IPhoneNumberService service = _service;
        var id = await SeedAsync("+46 70 444 44 44");

        // Act
        var act = async () => await service.UpdateAsync(id, CreateRequest("not-a-number"), null, TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
        _context.ChangeTracker.Clear();
        (await _context.PhoneNumbers.SingleAsync(TestContext.Current.CancellationToken)).Number.ShouldBe("+46 70 444 44 44");
    }

    [Fact]
    public async Task DeleteAsyncRemovesExistingPhoneNumber()
    {
        // Arrange
        IPhoneNumberService service = _service;
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
        IPhoneNumberService service = _service;
        await SeedAsync();

        // Act
        var result = await service.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
        (await CountStoredAsync()).ShouldBe(1);
    }
}
