using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Dotnetstore.JournalSystem.Api.Contact.Tests.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Addresses;

public sealed class AddressServiceTests : IDisposable
{
    private readonly DbContextOptions<ContactDataContext> _options;
    private readonly ContactDataContext _context;
    private readonly ContactUnitOfWork _unitOfWork;
    private readonly AddressService _service;
    private readonly Guid _addressTypeId = Guid.NewGuid();

    public AddressServiceTests(SqlServerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(_options);
        _context.Database.EnsureCreated();
        _context.AddressTypes.Add(AddressTypeTestData.Create(_addressTypeId, "Home"));
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
        _unitOfWork = new ContactUnitOfWork(_context, new AddressTypeRepository(_context), new AddressRepository(_context));
        _service = new AddressService(_unitOfWork);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
        using var cleanup = new ContactDataContext(_options);
        cleanup.Database.EnsureDeleted();
    }

    private EditAddressRequest CreateRequest(string street1 = "Main Street 1", Guid? addressTypeId = null)
    {
        return new EditAddressRequest(addressTypeId ?? _addressTypeId, AddressTestData.ObjectId, "c/o Anna", street1, "Floor 2", "12345", "Stockholm", "Stockholm County", "Sweden");
    }

    private async Task<Guid> SeedAsync(string street1 = "Seeded Street")
    {
        IAddressService service = _service;
        var result = await service.CreateAsync(CreateRequest(street1), null, TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
        return result.Value.Id;
    }

    private async Task<int> CountStoredAsync()
    {
        return await _context.Addresses.AsNoTracking().CountAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllAddressesAsResponses()
    {
        // Arrange
        IAddressService service = _service;
        await SeedAsync("A");
        await SeedAsync("B");

        // Act
        var result = await service.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Street1).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoAddressesExist()
    {
        // Arrange
        IAddressService service = _service;

        // Act
        var result = await service.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsyncReturnsAddressAsResponse()
    {
        // Arrange
        IAddressService service = _service;
        var id = await SeedAsync("Find Me Street");

        // Act
        var result = await service.GetByIdAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(id);
        result.Value.Street1.ShouldBe("Find Me Street");
        result.Value.AddressTypeId.ShouldBe(_addressTypeId);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNotFoundForUnknownId()
    {
        // Arrange
        IAddressService service = _service;
        await SeedAsync();

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task CreateAsyncPersistsAddressAndReturnsResponse()
    {
        // Arrange
        IAddressService service = _service;
        var userId = Guid.NewGuid();

        // Act
        var result = await service.CreateAsync(CreateRequest(), userId, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Street1.ShouldBe("Main Street 1");
        result.Value.CO.ShouldBe("c/o Anna");
        result.Value.Id.ShouldNotBe(Guid.Empty);
        _context.ChangeTracker.Clear();
        var stored = await _context.Addresses.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(result.Value.Id);
        stored.CreatedBy.ShouldBe(userId);
    }

    [Fact]
    public async Task CreateAsyncReturnsInvalidAndPersistsNothingForUnknownAddressType()
    {
        // Arrange
        IAddressService service = _service;

        // Act
        var result = await service.CreateAsync(CreateRequest(addressTypeId: Guid.NewGuid()), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.ShouldHaveSingleItem().Identifier.ShouldBe("AddressTypeId");
        (await CountStoredAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task CreateAsyncThrowsAndPersistsNothingForInvalidRequest()
    {
        // Arrange
        IAddressService service = _service;

        // Act
        var act = async () => await service.CreateAsync(CreateRequest(""), null, TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
        (await CountStoredAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task UpdateAsyncPersistsChangesAndReturnsResponse()
    {
        // Arrange
        IAddressService service = _service;
        var id = await SeedAsync();
        var editor = Guid.NewGuid();

        // Act
        var result = await service.UpdateAsync(id, CreateRequest("Renamed Street"), editor, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Street1.ShouldBe("Renamed Street");
        _context.ChangeTracker.Clear();
        var stored = await _context.Addresses.SingleAsync(TestContext.Current.CancellationToken);
        stored.Street1.ShouldBe("Renamed Street");
        stored.LastUpdatedBy.ShouldBe(editor);
        stored.LastUpdatedDate.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateAsyncReturnsNotFoundForUnknownId()
    {
        // Arrange
        IAddressService service = _service;

        // Act
        var result = await service.UpdateAsync(Guid.NewGuid(), CreateRequest(), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task UpdateAsyncReturnsInvalidAndKeepsStoredValuesForUnknownAddressType()
    {
        // Arrange
        IAddressService service = _service;
        var id = await SeedAsync("Original Street");

        // Act
        var result = await service.UpdateAsync(id, CreateRequest("Changed Street", Guid.NewGuid()), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Invalid);
        _context.ChangeTracker.Clear();
        (await _context.Addresses.SingleAsync(TestContext.Current.CancellationToken)).Street1.ShouldBe("Original Street");
    }

    [Fact]
    public async Task UpdateAsyncKeepsStoredValuesWhenRequestIsInvalid()
    {
        // Arrange
        IAddressService service = _service;
        var id = await SeedAsync("Original Street");

        // Act
        var act = async () => await service.UpdateAsync(id, CreateRequest(""), null, TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<ArgumentException>(act);
        _context.ChangeTracker.Clear();
        (await _context.Addresses.SingleAsync(TestContext.Current.CancellationToken)).Street1.ShouldBe("Original Street");
    }

    [Fact]
    public async Task DeleteAsyncRemovesExistingAddress()
    {
        // Arrange
        IAddressService service = _service;
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
        IAddressService service = _service;
        await SeedAsync();

        // Act
        var result = await service.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
        (await CountStoredAsync()).ShouldBe(1);
    }
}
