using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;

public sealed class OwnCompanyRepositoryTests : IDisposable
{
    private readonly OrganizationDataContext _context;
    private readonly OwnCompanyRepository _repository;

    public OwnCompanyRepositoryTests(SqlServerFixture fixture)
    {
        var options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new OrganizationDataContext(options);
        _context.Database.EnsureCreated();
        _repository = new OwnCompanyRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static OwnCompany CreateCompany(string name = "Dotnetstore AB")
    {
        return new OwnCompany(Guid.NewGuid(), name, "Description", null, new Uri("https://example.com"), "556677-8899", "SE556677889901");
    }

    private async Task SeedAsync(params OwnCompany[] companies)
    {
        _context.OwnCompanies.AddRange(companies);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
    }

    [Fact]
    public void RepositoryIsAGenericRepositoryOfOwnCompany()
    {
        // Arrange

        // Act
        var isGenericRepository = _repository is IGenericRepository<OwnCompany>;

        // Assert
        isGenericRepository.ShouldBeTrue();
    }

    [Fact]
    public async Task AddAsyncPersistsOwnCompanyWithAllValues()
    {
        // Arrange
        var company = CreateCompany();

        // Act
        await _repository.AddAsync(company, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.OwnCompanies.SingleAsync(TestContext.Current.CancellationToken);
        stored.Id.ShouldBe(company.Id);
        stored.Name.ShouldBe("Dotnetstore AB");
        stored.WebsiteUrl.ShouldBe(new Uri("https://example.com"));
        stored.CorporateNumber.ShouldBe("556677-8899");
        stored.TaxId.ShouldBe("SE556677889901");
    }

    [Fact]
    public async Task AddAsyncDoesNotPersistWhenChangesAreNotSaved()
    {
        // Arrange
        var company = CreateCompany();

        // Act
        await _repository.AddAsync(company, TestContext.Current.CancellationToken);

        // Assert
        (await _context.OwnCompanies.AsNoTracking().CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsOwnCompanyWhenItExists()
    {
        // Arrange
        var company = CreateCompany();
        await SeedAsync(company);

        // Act
        var result = await _repository.GetByIdAsync(company.Id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(company.Name);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNullWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateCompany());

        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllOwnCompanies()
    {
        // Arrange
        await SeedAsync(CreateCompany("A"), CreateCompany("B"));

        // Act
        var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Name).ShouldBe(["A", "B"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetAllAsyncReturnsEmptyWhenNoOwnCompaniesExist()
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
        var company = CreateCompany();
        await SeedAsync(company);
        var changed = new OwnCompany(company.Id, "Renamed AB", null, null, null, null, null);

        // Act
        _repository.Update(changed);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        _context.ChangeTracker.Clear();
        var stored = await _context.OwnCompanies.SingleAsync(TestContext.Current.CancellationToken);
        stored.Name.ShouldBe("Renamed AB");
        stored.TaxId.ShouldBeNull();
    }

    [Fact]
    public async Task DeleteAsyncRemovesOwnCompanyWhenItExists()
    {
        // Arrange
        var company = CreateCompany();
        await SeedAsync(company);

        // Act
        await _repository.DeleteAsync(company.Id, TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.OwnCompanies.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteAsyncKeepsOwnCompaniesWhenIdIsUnknown()
    {
        // Arrange
        await SeedAsync(CreateCompany());

        // Act
        await _repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        (await _context.OwnCompanies.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }
}
