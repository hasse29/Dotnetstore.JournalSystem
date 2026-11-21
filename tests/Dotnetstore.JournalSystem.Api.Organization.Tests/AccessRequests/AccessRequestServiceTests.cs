using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Api.Organization.AccessRequests;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.AccessRequests;

public sealed class AccessRequestServiceTests : IDisposable
{
    private static readonly EntraIdentity Anna = new("oid-anna", "Anna Maria Svensson", "anna@example.com");

    private readonly OrganizationTestDatabase _database;
    private readonly IAccessRequestService _service;
    private readonly Guid _companyId = Guid.NewGuid();
    private readonly Guid _roleId = Guid.NewGuid();

    public AccessRequestServiceTests(SqlServerFixture fixture)
    {
        _database = new OrganizationTestDatabase(fixture);
        _service = new AccessRequestService(_database.UnitOfWork, new PersonalIdentityNumberProtector(TestEncryption.Create()), TimeProvider.System);

        _database.Context.OwnCompanies.Add(OwnCompanyTestData.Create(_companyId, "Dotnetstore AB", null, null, null, null, null));
        _database.Context.Roles.Add(RoleTestData.Create(_roleId, "Doctor"));
        _database.Context.SaveChanges();
        _database.Context.ChangeTracker.Clear();
    }

    public void Dispose() => _database.Dispose();

    private ApproveAccessRequestRequest Approval() => new(_companyId, _roleId, "Anna", null, "Svensson", false);

    private async Task<Guid> RequestAsync(EntraIdentity? identity = null)
    {
        var result = await _service.RequestAccessAsync(identity ?? Anna, TestContext.Current.CancellationToken);
        _database.Context.ChangeTracker.Clear();
        return result.Value.Id;
    }

    [Fact]
    public async Task RequestAccessRecordsAPendingRequest()
    {
        // Arrange & Act
        var result = await _service.RequestAccessAsync(Anna, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe(AccessRequestStatus.Pending);
        result.Value.DisplayName.ShouldBe("Anna Maria Svensson");
        var stored = await _database.Context.AccessRequests.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        stored.ExternalId.ShouldBe("oid-anna");
    }

    [Fact]
    public async Task RequestAccessTwiceReturnsTheSameRequestInsteadOfADuplicate()
    {
        // Arrange
        var first = await RequestAsync();

        // Act
        var second = await _service.RequestAccessAsync(Anna, TestContext.Current.CancellationToken);

        // Assert
        second.Value.Id.ShouldBe(first);
        (await _database.Context.AccessRequests.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task RequestAccessReturnsTheRejectionToAPersonWhoWasRejected()
    {
        // Arrange
        var id = await RequestAsync();
        await _service.RejectAsync(id, new RejectAccessRequestRequest("No"), null, TestContext.Current.CancellationToken);
        _database.Context.ChangeTracker.Clear();

        // Act
        var result = await _service.RequestAccessAsync(Anna, TestContext.Current.CancellationToken);

        // Assert
        result.Value.Status.ShouldBe(AccessRequestStatus.Rejected);
        result.Value.DecisionNote.ShouldBe("No");
    }

    [Fact]
    public async Task RequestAccessForSomeoneWhoAlreadyHasAnAccountReportsApprovedWithoutRecordingARequest()
    {
        // Arrange
        _database.Context.Users.Add(UserTestData.Create(Guid.NewGuid(), "anna@example.com", AuthProvider.Entra, "oid-anna"));
        await _database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.RequestAccessAsync(Anna, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe(AccessRequestStatus.Approved);
        (await _database.Context.AccessRequests.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task GetMineReturnsTheCallersRequestAndNotFoundForStrangers()
    {
        // Arrange
        await RequestAsync();

        // Act
        var mine = await _service.GetMineAsync("oid-anna", TestContext.Current.CancellationToken);
        var other = await _service.GetMineAsync("oid-nobody", TestContext.Current.CancellationToken);

        // Assert
        mine.IsSuccess.ShouldBeTrue();
        other.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task GetMineReportsApprovedForAnAdministratorCreatedUserWhoNeverApplied()
    {
        // Arrange
        _database.Context.Users.Add(UserTestData.Create(Guid.NewGuid(), "anna@example.com", AuthProvider.Entra, "oid-anna"));
        await _database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetMineAsync("oid-anna", TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe(AccessRequestStatus.Approved);
        result.Value.Username.ShouldBe("anna@example.com");
    }

    [Fact]
    public async Task GetAllFiltersByStatusAndOrdersOldestFirst()
    {
        // Arrange
        var annaId = await RequestAsync();
        await RequestAsync(new EntraIdentity("oid-bo", "Bo Berg", "bo@example.com"));
        await _service.RejectAsync(annaId, new RejectAccessRequestRequest(null), null, TestContext.Current.CancellationToken);
        _database.Context.ChangeTracker.Clear();

        // Act
        var all = await _service.GetAllAsync(null, TestContext.Current.CancellationToken);
        var pending = await _service.GetAllAsync(AccessRequestStatus.Pending, TestContext.Current.CancellationToken);

        // Assert
        all.Count.ShouldBe(2);
        pending.ShouldHaveSingleItem().Username.ShouldBe("bo@example.com");
    }

    [Fact]
    public async Task ApproveCreatesAnApprovedEntraUserWithTheChosenRoleAndCompany()
    {
        // Arrange
        var id = await RequestAsync();
        var admin = Guid.NewGuid();

        // Act
        var result = await _service.ApproveAsync(id, Approval(), admin, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Status.ShouldBe(AccessRequestStatus.Approved);
        var user = await _database.Context.Users.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        user.ExternalId.ShouldBe("oid-anna");
        user.AuthProvider.ShouldBe(AuthProvider.Entra);
        user.AccountIsApproved.ShouldBeTrue();
        user.RoleId.ShouldBe(_roleId);
        user.OwnCompanyId.ShouldBe(_companyId);
        user.Username.ShouldBe("anna@example.com");
        user.PasswordHash.ShouldBeEmpty();
        user.FirstName.ShouldBe("Anna");
        user.LastName.ShouldBe("Svensson");
        user.CreatedBy.ShouldBe(admin);
    }

    [Fact]
    public async Task ApproveTwiceIsRejectedAndCreatesNoSecondUser()
    {
        // Arrange
        var id = await RequestAsync();
        await _service.ApproveAsync(id, Approval(), null, TestContext.Current.CancellationToken);
        _database.Context.ChangeTracker.Clear();

        // Act
        var result = await _service.ApproveAsync(id, Approval(), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Conflict);
        (await _database.Context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task ApproveFailsForAnUnknownRequest()
    {
        // Arrange & Act
        var result = await _service.ApproveAsync(Guid.NewGuid(), Approval(), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task ApproveWithAnUnknownRoleOrCompanyCreatesNoUserAndLeavesTheRequestPending()
    {
        // Arrange
        var id = await RequestAsync();

        // Act
        var result = await _service.ApproveAsync(id, new ApproveAccessRequestRequest(Guid.NewGuid(), Guid.NewGuid(), "Anna", null, "Svensson", false), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Invalid);
        result.ValidationErrors.Select(x => x.Identifier).ShouldBe([nameof(ApproveAccessRequestRequest.OwnCompanyId), nameof(ApproveAccessRequestRequest.RoleId)]);
        (await _database.Context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
        var stored = await _database.Context.AccessRequests.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        stored.Status.ShouldBe(AccessRequestStatus.Pending);
    }

    [Fact]
    public async Task ApproveIsRefusedWhenTheUsernameIsAlreadyTaken()
    {
        // Arrange
        _database.Context.Users.Add(UserTestData.Create(Guid.NewGuid(), "anna@example.com"));
        await _database.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var id = await RequestAsync();

        // Act
        var result = await _service.ApproveAsync(id, Approval(), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Conflict);
        var stored = await _database.Context.AccessRequests.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        stored.Status.ShouldBe(AccessRequestStatus.Pending);
    }

    [Fact]
    public async Task RejectRecordsTheNoteAndCreatesNoUser()
    {
        // Arrange
        var id = await RequestAsync();

        // Act
        var result = await _service.RejectAsync(id, new RejectAccessRequestRequest("Not part of the clinic"), Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Value.Status.ShouldBe(AccessRequestStatus.Rejected);
        result.Value.DecisionNote.ShouldBe("Not part of the clinic");
        (await _database.Context.Users.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task RejectIsRefusedForARequestThatWasAlreadyApproved()
    {
        // Arrange
        var id = await RequestAsync();
        await _service.ApproveAsync(id, Approval(), null, TestContext.Current.CancellationToken);
        _database.Context.ChangeTracker.Clear();

        // Act
        var result = await _service.RejectAsync(id, new RejectAccessRequestRequest("late"), null, TestContext.Current.CancellationToken);

        // Assert
        result.Status.ShouldBe(Ardalis.Result.ResultStatus.Conflict);
    }

    [Fact]
    public async Task ReopenPutsARejectedRequestBackToPendingSoItCanBeApproved()
    {
        // Arrange
        var id = await RequestAsync();
        await _service.RejectAsync(id, new RejectAccessRequestRequest("No"), null, TestContext.Current.CancellationToken);
        _database.Context.ChangeTracker.Clear();

        // Act
        var reopened = await _service.ReopenAsync(id, TestContext.Current.CancellationToken);
        _database.Context.ChangeTracker.Clear();
        var approved = await _service.ApproveAsync(id, Approval(), null, TestContext.Current.CancellationToken);

        // Assert
        reopened.Value.Status.ShouldBe(AccessRequestStatus.Pending);
        reopened.Value.DecisionNote.ShouldBeNull();
        approved.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ReopenIsRefusedForAPendingRequestAndForAnUnknownOne()
    {
        // Arrange
        var id = await RequestAsync();

        // Act
        var pending = await _service.ReopenAsync(id, TestContext.Current.CancellationToken);
        var unknown = await _service.ReopenAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        pending.Status.ShouldBe(Ardalis.Result.ResultStatus.Conflict);
        unknown.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task ApproveStoresTheChosenPersonDetailsWithTheNumberEncrypted()
    {
        // Arrange
        var id = await RequestAsync();
        var request = new ApproveAccessRequestRequest(_companyId, _roleId, "Anna", "Maria", "Svensson", false, new DateOnly(1990, 1, 1), "19900101-1234");

        // Act
        var result = await _service.ApproveAsync(id, request, null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var user = await _database.Context.Users.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        user.FirstName.ShouldBe("Anna");
        user.MiddleName.ShouldBe("Maria");
        user.LastName.ShouldBe("Svensson");
        user.IsMale.ShouldBeFalse();
        user.DateOfBirth.ShouldNotBeNull().ShouldBe(new DateTime(1990, 1, 1));
        user.SocialSecurityNumber.ShouldNotBeNullOrWhiteSpace();
        user.SocialSecurityNumber.ShouldNotContain("19900101");
    }

    [Fact]
    public async Task ApproveWithoutOptionalDetailsLeavesThemEmpty()
    {
        // Arrange
        var id = await RequestAsync();

        // Act
        var result = await _service.ApproveAsync(id, Approval(), null, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var user = await _database.Context.Users.AsNoTracking().SingleAsync(TestContext.Current.CancellationToken);
        user.DateOfBirth.ShouldBeNull();
        user.SocialSecurityNumber.ShouldBeNull();
    }
}
