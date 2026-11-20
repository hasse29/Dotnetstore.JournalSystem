using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.Seeding;
using Dotnetstore.JournalSystem.Api.Contact.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Seeding;

public sealed class ContactSeederTests : IDisposable
{
    private const string SeedJson = """
        {
          "addressTypes": [ { "name": "Besök" }, { "name": "Hem" } ],
          "phoneNumberTypes": [ { "name": "Mobil" } ],
          "emailAddressTypes": [ { "name": "Arbete" } ]
        }
        """;

    private readonly DbContextOptions<ContactDataContext> _options;
    private readonly ContactDataContext _context;
    private readonly string _seedFile = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid():N}.json");

    public ContactSeederTests(SqlServerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new ContactDataContext(_options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        using var cleanup = new ContactDataContext(_options);
        cleanup.Database.EnsureDeleted();
        File.Delete(_seedFile);
    }

    [Fact]
    public async Task SeedAsyncAddsAllTypesAsSystem()
    {
        // Arrange
        await File.WriteAllTextAsync(_seedFile, SeedJson, TestContext.Current.CancellationToken);

        // Act
        var added = await ContactSeeder.SeedAsync(_context, [_seedFile], TestContext.Current.CancellationToken);

        // Assert
        added.ShouldBe(4);
        var addressTypes = await _context.AddressTypes.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken);
        addressTypes.Select(x => x.Name).ShouldBe(["Besök", "Hem"], ignoreOrder: true);
        addressTypes.ShouldAllBe(x => x.IsSystem);
        (await _context.PhoneNumberTypes.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken)).IsSystem.ShouldBeTrue();
        (await _context.EmailAddressTypes.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken)).IsSystem.ShouldBeTrue();
    }

    [Fact]
    public async Task SeedAsyncIsIdempotent()
    {
        // Arrange
        await File.WriteAllTextAsync(_seedFile, SeedJson, TestContext.Current.CancellationToken);
        await ContactSeeder.SeedAsync(_context, [_seedFile], TestContext.Current.CancellationToken);

        // Act
        var added = await ContactSeeder.SeedAsync(_context, [_seedFile], TestContext.Current.CancellationToken);

        // Assert
        added.ShouldBe(0);
        (await _context.AddressTypes.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(2);
    }

    [Fact]
    public async Task SeedAsyncDoesNothingWhenFileIsMissing()
    {
        // Arrange
        var missing = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

        // Act
        var added = await ContactSeeder.SeedAsync(_context, [missing], TestContext.Current.CancellationToken);

        // Assert
        added.ShouldBe(0);
        (await _context.AddressTypes.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }
}
