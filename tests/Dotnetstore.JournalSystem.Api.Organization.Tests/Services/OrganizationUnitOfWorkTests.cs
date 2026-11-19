using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Microsoft.EntityFrameworkCore;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Services;

public sealed class OrganizationUnitOfWorkTests : IDisposable
{
    private readonly DbContextOptions<OrganizationDataContext> _options;
    private readonly OrganizationDataContext _context;
    private readonly OwnCompanyRepository _repository;
    private readonly OrganizationUnitOfWork _unitOfWork;

    public OrganizationUnitOfWorkTests(SqlServerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new OrganizationDataContext(_options);
        _context.Database.EnsureCreated();
        _repository = new OwnCompanyRepository(_context);
        _unitOfWork = new OrganizationUnitOfWork(_context, _repository);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
        using var cleanup = new OrganizationDataContext(_options);
        cleanup.Database.EnsureDeleted();
    }

    private static OwnCompany CreateCompany()
    {
        return OwnCompanyTestData.Create(Guid.NewGuid(), "Dotnetstore AB", null, null, null, null, null);
    }

    private async Task<int> CountStoredAsync()
    {
        return await _context.OwnCompanies.AsNoTracking().CountAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void OwnCompaniesReturnsTheInjectedRepository()
    {
        // Arrange
        IOrganizationUnitOfWork unitOfWork = _unitOfWork;

        // Act
        var result = unitOfWork.OwnCompanies;

        // Assert
        result.ShouldBeSameAs(_repository);
    }

    [Fact]
    public async Task SaveChangesAsyncPersistsAndReturnsNumberOfChanges()
    {
        // Arrange
        IOrganizationUnitOfWork unitOfWork = _unitOfWork;
        await unitOfWork.OwnCompanies.AddAsync(CreateCompany(), TestContext.Current.CancellationToken);

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
        IOrganizationUnitOfWork unitOfWork = _unitOfWork;

        // Act
        var changes = await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        changes.ShouldBe(0);
    }

    [Fact]
    public async Task SaveChangesAsyncThrowsOriginalExceptionTypeOnFailure()
    {
        // Arrange
        IOrganizationUnitOfWork unitOfWork = _unitOfWork;
        var company = CreateCompany();
        await unitOfWork.OwnCompanies.AddAsync(company, TestContext.Current.CancellationToken);
        await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
        await unitOfWork.OwnCompanies.AddAsync(
            OwnCompanyTestData.Create(company.Id, "Duplicate", null, null, null, null, null),
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
        IOrganizationUnitOfWork unitOfWork = _unitOfWork;
        unitOfWork.CreateTransaction();
        await unitOfWork.OwnCompanies.AddAsync(CreateCompany(), TestContext.Current.CancellationToken);
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
        IOrganizationUnitOfWork unitOfWork = _unitOfWork;
        unitOfWork.CreateTransaction();
        await unitOfWork.OwnCompanies.AddAsync(CreateCompany(), TestContext.Current.CancellationToken);
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
        IOrganizationUnitOfWork unitOfWork = _unitOfWork;

        // Act
        var act = async () => await unitOfWork.CommitAsync(TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<InvalidOperationException>(act);
    }

    [Fact]
    public async Task RollbackAsyncThrowsWhenNoTransactionWasCreated()
    {
        // Arrange
        IOrganizationUnitOfWork unitOfWork = _unitOfWork;

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
        Should.Throw<ObjectDisposedException>(() => _context.OwnCompanies.ToList());
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
