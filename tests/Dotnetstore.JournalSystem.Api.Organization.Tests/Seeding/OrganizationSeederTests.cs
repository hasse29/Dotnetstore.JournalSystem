using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.Seeding;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Seeding;

public sealed class OrganizationSeederTests : IDisposable
{
    private const string SeedJson = """
        {
          "ownCompanies": [
            {
              "name": "Seeded Company",
              "description": "Owner of the application",
              "websiteUrl": "https://www.example.com/",
              "taxId": "000000-0000"
            }
          ]
        }
        """;

    private readonly DbContextOptions<OrganizationDataContext> _options;
    private readonly OrganizationDataContext _context;
    private readonly string _seedFile = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid():N}.json");

    public OrganizationSeederTests(SqlServerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new OrganizationDataContext(_options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        using var cleanup = new OrganizationDataContext(_options);
        cleanup.Database.EnsureDeleted();
        File.Delete(_seedFile);
    }

    [Fact]
    public async Task SeedAsyncAddsOwnCompanyAsSystem()
    {
        // Arrange
        await File.WriteAllTextAsync(_seedFile, SeedJson, TestContext.Current.CancellationToken);

        // Act
        var added = await OrganizationSeeder.SeedAsync(_context, [_seedFile], TestContext.Current.CancellationToken);

        // Assert
        added.ShouldBe(1);
        var stored = await _context.OwnCompanies.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        stored.Name.ShouldBe("Seeded Company");
        stored.TaxId.ShouldBe("000000-0000");
        stored.WebsiteUrl.ShouldBe(new Uri("https://www.example.com/"));
        stored.IsSystem.ShouldBeTrue();
    }

    [Fact]
    public async Task SeedAsyncIsIdempotent()
    {
        // Arrange
        await File.WriteAllTextAsync(_seedFile, SeedJson, TestContext.Current.CancellationToken);
        await OrganizationSeeder.SeedAsync(_context, [_seedFile], TestContext.Current.CancellationToken);

        // Act
        var added = await OrganizationSeeder.SeedAsync(_context, [_seedFile], TestContext.Current.CancellationToken);

        // Assert
        added.ShouldBe(0);
        (await _context.OwnCompanies.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task SeedAsyncDoesNothingWhenFileIsMissing()
    {
        // Arrange
        var missing = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

        // Act
        var added = await OrganizationSeeder.SeedAsync(_context, [missing], TestContext.Current.CancellationToken);

        // Assert
        added.ShouldBe(0);
        (await _context.OwnCompanies.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task SeedAsyncReadsAllFilesAndSkipsMissingOnes()
    {
        // Arrange
        var secondFile = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid():N}.json");
        var missing = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(_seedFile, SeedJson, TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(
            secondFile,
            SeedJson.Replace("Seeded Company", "Second Company", StringComparison.Ordinal),
            TestContext.Current.CancellationToken);

        try
        {
            // Act
            var added = await OrganizationSeeder.SeedAsync(_context, [_seedFile, missing, secondFile], TestContext.Current.CancellationToken);

            // Assert
            added.ShouldBe(2);
            var names = await _context.OwnCompanies.AsNoTracking().Select(x => x.Name).ToListAsync(TestContext.Current.CancellationToken);
            names.ShouldBe(["Seeded Company", "Second Company"], ignoreOrder: true);
        }
        finally
        {
            File.Delete(secondFile);
        }
    }
}
