using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.Security.Extensions;
using Dotnetstore.Security.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class UserServiceTests : IDisposable
{
    private readonly DbContextOptions<OrganizationDataContext> _options;
    private readonly OrganizationDataContext _context;
    private readonly OrganizationUnitOfWork _unitOfWork;
    private readonly FakeEntraUserProvisioner _entra = new();
    private readonly ServiceProvider _provider;
    private readonly IHashHandler _hashHandler;
    private readonly UserService _service;
    private readonly Guid _ownCompanyId = Guid.NewGuid();
    private readonly Guid _roleId = Guid.NewGuid();

    public UserServiceTests(SqlServerFixture fixture)
    {
        _options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString())
            .Options;

        _context = new OrganizationDataContext(_options);
        _context.Database.EnsureCreated();
        _unitOfWork = new OrganizationUnitOfWork(
            _context,
            new OwnCompanyRepository(_context),
            new RoleRepository(_context),
            new UserRepository(_context),
            new WorkGroupRepository(_context));

        _provider = new ServiceCollection().AddSecurity().BuildServiceProvider();
        _hashHandler = _provider.GetRequiredService<IHashHandler>();
        _service = new UserService(_unitOfWork, _hashHandler, _entra);

        _context.OwnCompanies.Add(OwnCompanyTestData.Create(_ownCompanyId, "Dotnetstore AB", null, null, null, null, null));
        _context.Roles.Add(RoleTestData.Create(_roleId, "Administrator"));
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
        _provider.Dispose();
        using var cleanup = new OrganizationDataContext(_options);
        cleanup.Database.EnsureDeleted();
    }

    private CreateUserRequest CreateRequest(
        string username = "anna@example.com",
        AuthProvider authProvider = AuthProvider.Local,
        string? password = "a-long-enough-password")
    {
        return UserTestData.CreateRequest(username, authProvider, password, _ownCompanyId, _roleId);
    }

    private async Task<Guid> SeedAsync(string username = "seed@example.com", AuthProvider authProvider = AuthProvider.Local)
    {
        IUserService service = _service;
        var password = authProvider == AuthProvider.Local ? "a-long-enough-password" : null;
        var result = await service.CreateAsync(CreateRequest(username, authProvider, password), null, TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();
        return result.Value.Id;
    }

    [Fact]
    public async Task CreateAsyncStoresLocalUserWithHashedPassword()
    {
        // Arrange
        IUserService service = _service;

        // Act
        var result = await service.CreateAsync(CreateRequest(), null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TemporaryPassword.ShouldBeNull();
        var stored = await _context.Users.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        stored.PasswordHash.ShouldNotBe("a-long-enough-password");
        _hashHandler.Verify("a-long-enough-password", stored.PasswordHash).ShouldBeTrue();
        stored.ExternalId.ShouldBeNull();
        _entra.Created.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateAsyncProvisionsEntraUserAndStoresExternalId()
    {
        // Arrange
        IUserService service = _service;
        _entra.NextObjectId = "oid-123";

        // Act
        var result = await service.CreateAsync(CreateRequest("bo@example.com", AuthProvider.Entra, null), null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TemporaryPassword.ShouldBe("Temp-Password-1!");
        var stored = await _context.Users.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        stored.ExternalId.ShouldBe("oid-123");
        stored.PasswordHash.ShouldBeEmpty();
        _entra.Created.ShouldHaveSingleItem().Username.ShouldBe("bo@example.com");
    }

    [Fact]
    public async Task CreateAsyncReturnsErrorAndStoresNothingWhenEntraFails()
    {
        // Arrange
        IUserService service = _service;
        _entra.Fail = true;

        // Act
        var result = await service.CreateAsync(CreateRequest("bo@example.com", AuthProvider.Entra, null), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Error);
        (await _context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task CreateAsyncRejectsDuplicateUsername()
    {
        // Arrange
        IUserService service = _service;
        await SeedAsync("anna@example.com");

        // Act
        var result = await service.CreateAsync(CreateRequest("anna@example.com"), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Conflict);
        (await _context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task CreateAsyncRejectsUnknownRoleAndOwnCompany()
    {
        // Arrange
        IUserService service = _service;
        var request = UserTestData.CreateRequest();

        // Act
        var result = await service.CreateAsync(request, null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.ValidationErrors.Select(x => x.Identifier).ShouldBe(
            [nameof(CreateUserRequest.OwnCompanyId), nameof(CreateUserRequest.RoleId)],
            ignoreOrder: true);
        (await _context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task GetAllAsyncReturnsAllUsers()
    {
        // Arrange
        IUserService service = _service;
        await SeedAsync("a@example.com");
        await SeedAsync("b@example.com");

        // Act
        var result = await service.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Select(x => x.Username).ShouldBe(["a@example.com", "b@example.com"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsUser()
    {
        // Arrange
        IUserService service = _service;
        var id = await SeedAsync();

        // Act
        var result = await service.GetByIdAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(id);
    }

    [Fact]
    public async Task GetByIdAsyncReturnsNotFoundForUnknownId()
    {
        // Arrange
        IUserService service = _service;

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task UpdateAsyncChangesNamesButNotCredentials()
    {
        // Arrange
        IUserService service = _service;
        var id = await SeedAsync();
        var before = await _context.Users.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        var request = new EditUserRequest("Bo", null, "Nilsson", _ownCompanyId, _roleId);

        // Act
        var result = await service.UpdateAsync(id, request, null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _context.ChangeTracker.Clear();
        var stored = await _context.Users.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        stored.FirstName.ShouldBe("Bo");
        stored.PasswordHash.ShouldBe(before.PasswordHash);
        stored.Username.ShouldBe(before.Username);
        _entra.Updated.ShouldBeEmpty();
    }

    [Fact]
    public async Task UpdateAsyncPropagatesChangesToEntraForEntraUsers()
    {
        // Arrange
        IUserService service = _service;
        _entra.NextObjectId = "oid-9";
        var id = await SeedAsync("bo@example.com", AuthProvider.Entra);
        var request = new EditUserRequest("Bo", null, "Nilsson", _ownCompanyId, _roleId);

        // Act
        var result = await service.UpdateAsync(id, request, null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _entra.Updated.ShouldHaveSingleItem().ExternalId.ShouldBe("oid-9");
    }

    [Fact]
    public async Task UpdateAsyncKeepsLocalDataUnchangedWhenEntraFails()
    {
        // Arrange
        IUserService service = _service;
        var id = await SeedAsync("bo@example.com", AuthProvider.Entra);
        _entra.Fail = true;
        var request = new EditUserRequest("Bo", null, "Nilsson", _ownCompanyId, _roleId);

        // Act
        var result = await service.UpdateAsync(id, request, null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Error);
        _context.ChangeTracker.Clear();
        (await _context.Users.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken)).FirstName.ShouldBe("Anna");
    }

    [Fact]
    public async Task UpdateAsyncReturnsNotFoundForUnknownId()
    {
        // Arrange
        IUserService service = _service;
        var request = new EditUserRequest("Bo", null, "Nilsson", _ownCompanyId, _roleId);

        // Act
        var result = await service.UpdateAsync(Guid.NewGuid(), request, null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task UpdateAsyncRejectsUnknownRole()
    {
        // Arrange
        IUserService service = _service;
        var id = await SeedAsync();
        var request = new EditUserRequest("Bo", null, "Nilsson", _ownCompanyId, Guid.NewGuid());

        // Act
        var result = await service.UpdateAsync(id, request, null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public async Task DeleteAsyncRemovesLocalUser()
    {
        // Arrange
        IUserService service = _service;
        var id = await SeedAsync();

        // Act
        var result = await service.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        (await _context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
        _entra.Deleted.ShouldBeEmpty();
    }

    [Fact]
    public async Task DeleteAsyncAlsoDeletesEntraUser()
    {
        // Arrange
        IUserService service = _service;
        _entra.NextObjectId = "oid-5";
        var id = await SeedAsync("bo@example.com", AuthProvider.Entra);

        // Act
        var result = await service.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        _entra.Deleted.ShouldHaveSingleItem().ShouldBe("oid-5");
    }

    [Fact]
    public async Task DeleteAsyncReturnsNotFoundForUnknownId()
    {
        // Arrange
        IUserService service = _service;

        // Act
        var result = await service.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task DeleteAsyncRejectsSystemUsers()
    {
        // Arrange
        IUserService service = _service;
        var id = Guid.NewGuid();
        _context.Users.Add(UserTestData.Create(id, isSystem: true));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);
        _context.ChangeTracker.Clear();

        // Act
        var result = await service.DeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(ResultStatus.Conflict);
        (await _context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    private sealed class FakeEntraUserProvisioner : IEntraUserProvisioner
    {
        internal string NextObjectId { get; set; } = "oid-default";
        internal bool Fail { get; set; }
        internal List<EntraUserDetails> Created { get; } = [];
        internal List<(string ExternalId, EntraUserDetails Details)> Updated { get; } = [];
        internal List<string> Deleted { get; } = [];

        ValueTask<Result<EntraUserCreated>> IEntraUserProvisioner.CreateAsync(EntraUserDetails details, CancellationToken cancellationToken)
        {
            if (Fail)
            {
                return ValueTask.FromResult(Result<EntraUserCreated>.Error("Entra failure."));
            }

            Created.Add(details);
            return ValueTask.FromResult(Result<EntraUserCreated>.Success(new EntraUserCreated(NextObjectId, "Temp-Password-1!")));
        }

        ValueTask<Result> IEntraUserProvisioner.UpdateAsync(string externalId, EntraUserDetails details, CancellationToken cancellationToken)
        {
            if (Fail)
            {
                return ValueTask.FromResult(Result.Error("Entra failure."));
            }

            Updated.Add((externalId, details));
            return ValueTask.FromResult(Result.Success());
        }

        ValueTask<Result> IEntraUserProvisioner.DeleteAsync(string externalId, CancellationToken cancellationToken)
        {
            Deleted.Add(externalId);
            return ValueTask.FromResult(Result.Success());
        }
    }
}
