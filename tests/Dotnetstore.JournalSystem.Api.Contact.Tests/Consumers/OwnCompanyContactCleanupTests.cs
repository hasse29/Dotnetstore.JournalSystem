using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.Consumers;
using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Dotnetstore.JournalSystem.Api.Contact.Services;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Consumers;

public sealed class OwnCompanyContactCleanupTests : IDisposable
{
    private readonly DbContextOptions<ContactDataContext> _options;
    private readonly ContactDataContext _context;
    private readonly ContactUnitOfWork _unitOfWork;
    private readonly OwnCompanyContactCleanup _cleanup;

    public OwnCompanyContactCleanupTests(SqlServerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(_options);
        _context.Database.EnsureCreated();
        _unitOfWork = new ContactUnitOfWork(
            _context,
            new AddressTypeRepository(_context),
            new AddressRepository(_context),
            new EmailAddressTypeRepository(_context),
            new EmailAddressRepository(_context),
            new PhoneNumberTypeRepository(_context),
            new PhoneNumberRepository(_context));
        _cleanup = new OwnCompanyContactCleanup(_unitOfWork);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
        using var cleanup = new ContactDataContext(_options);
        cleanup.Database.EnsureDeleted();
    }

    private async Task SeedAsync(Guid objectId)
    {
        _context.Addresses.Add(AddressTestData.Create(Guid.NewGuid(), objectId: objectId));
        _context.EmailAddresses.Add(EmailAddressTestData.Create(Guid.NewGuid(), objectId: objectId));
        _context.PhoneNumbers.Add(PhoneNumberTestData.Create(Guid.NewGuid(), objectId: objectId));
        await _context.SaveChangesAsync(Token);
        _context.ChangeTracker.Clear();
    }

    private async Task<Guid[]> StoredObjectIdsAsync()
    {
        using var context = new ContactDataContext(_options);
        var addresses = await context.Addresses.Select(x => x.ObjectId).ToListAsync(Token);
        var emailAddresses = await context.EmailAddresses.Select(x => x.ObjectId).ToListAsync(Token);
        var phoneNumbers = await context.PhoneNumbers.Select(x => x.ObjectId).ToListAsync(Token);
        return [.. addresses, .. emailAddresses, .. phoneNumbers];
    }

    [Fact]
    public async Task DeleteForCompanyAsyncRemovesEveryKindOfContactDataOfThatCompanyOnly()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        await SeedAsync(companyId);
        await SeedAsync(otherId);

        // Act
        await _cleanup.DeleteForCompanyAsync(companyId, Token);

        // Assert
        (await StoredObjectIdsAsync()).ShouldBe([otherId, otherId, otherId]);
    }

    [Fact]
    public async Task DeleteForCompanyAsyncKeepsEverythingForACompanyWithoutContactData()
    {
        // Arrange
        await SeedAsync(Guid.NewGuid());

        // Act
        await _cleanup.DeleteForCompanyAsync(Guid.NewGuid(), Token);

        // Assert
        (await StoredObjectIdsAsync()).Length.ShouldBe(3);
    }

    [Fact]
    public async Task DeleteForCompanyAsyncCanRunAgainForTheSameCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        await SeedAsync(companyId);
        await _cleanup.DeleteForCompanyAsync(companyId, Token);
        _context.ChangeTracker.Clear();

        // Act
        var again = () => _cleanup.DeleteForCompanyAsync(companyId, Token);

        // Assert
        await again.ShouldNotThrowAsync();
        (await StoredObjectIdsAsync()).ShouldBeEmpty();
    }
}
