using System.Net;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Roles;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Pages.Roles;

public sealed class IndexModelTests
{
    private sealed class FakeRoleClient : IRoleClient
    {
        public Func<ApiResult<IReadOnlyList<RoleResponse>>> List { get; init; } =
            () => ApiResult.Create<IReadOnlyList<RoleResponse>>(HttpStatusCode.OK, []);

        public Func<ApiResult<RoleResponse>> Write { get; init; } =
            () => ApiResult.Create<RoleResponse>(HttpStatusCode.OK, null);

        public Func<ApiResult> Remove { get; init; } = () => ApiResult.Create(HttpStatusCode.NoContent);

        public EditRoleRequest? Sent { get; private set; }

        public Guid? DeletedId { get; private set; }

        public Task<ApiResult<IReadOnlyList<RoleResponse>>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(List());

        public Task<ApiResult<RoleResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ApiResult<RoleResponse>> CreateAsync(EditRoleRequest request, CancellationToken cancellationToken = default)
        {
            Sent = request;
            return Task.FromResult(Write());
        }

        public Task<ApiResult<RoleResponse>> UpdateAsync(Guid id, EditRoleRequest request, CancellationToken cancellationToken = default)
        {
            Sent = request;
            return Task.FromResult(Write());
        }

        public Task<ApiResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            DeletedId = id;
            return Task.FromResult(Remove());
        }
    }

    private static IndexModel Create(FakeRoleClient client, FakeCurrentUserProfile? profile = null) =>
        new(client, profile ?? FakeCurrentUserProfile.Administrator())
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
        };

    private static ApiProblem Problem(string detail) => new(null, detail, new Dictionary<string, string[]>());

    private static FakeRoleClient Listing(HttpStatusCode status, params string[] names) => new()
    {
        List = () => ApiResult.Create<IReadOnlyList<RoleResponse>>(
            status,
            names.Select(x => new RoleResponse(Guid.NewGuid(), x, string.Empty)).ToList())
    };

    [Fact]
    public async Task ListsAllRolesSortedByName()
    {
        // Arrange
        var model = Create(Listing(HttpStatusCode.OK, "Porter", "doctor", "Nurse"));

        // Act
        var result = await model.OnGetAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeOfType<PageResult>();
        model.Unavailable.ShouldBeFalse();
        model.Roles.Select(x => x.Name).ShouldBe(["doctor", "Nurse", "Porter"]);
    }

    [Fact]
    public async Task ReportsUnavailableWhenTheApiRefuses()
    {
        // Arrange
        var model = Create(Listing(HttpStatusCode.InternalServerError));

        // Act
        await model.OnGetAsync(TestContext.Current.CancellationToken);

        // Assert
        model.Unavailable.ShouldBeTrue();
        model.Roles.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReportsUnavailableWhenTheApiCannotBeReached()
    {
        // Arrange
        var model = Create(new FakeRoleClient { List = () => throw new HttpRequestException("down") });

        // Act
        await model.OnGetAsync(TestContext.Current.CancellationToken);

        // Assert
        model.Unavailable.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateSendsTheTrimmedValuesAndReportsSuccess()
    {
        // Arrange
        var client = new FakeRoleClient();
        var model = Create(client);

        // Act
        var result = await model.OnPostCreateAsync(new RoleInput { Name = "  Nurse ", Description = " Care " }, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeOfType<RedirectToPageResult>();
        model.Message.ShouldBe("Role created.");
        var sent = client.Sent.ShouldNotBeNull();
        sent.Name.ShouldBe("Nurse");
        sent.Description.ShouldBe("Care");
        sent.Permissions.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateSendsAnEmptyDescriptionWhenNoneIsGiven()
    {
        // Arrange
        var client = new FakeRoleClient();
        var model = Create(client);

        // Act
        await model.OnPostCreateAsync(new RoleInput { Name = "Nurse" }, TestContext.Current.CancellationToken);

        // Assert
        client.Sent.ShouldNotBeNull().Description.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("  ", 0)]
    [InlineData("Nurse", DataSchemeConstants.RoleDescriptionMaxLength + 1)]
    public async Task CreateRejectsInvalidInputWithoutCallingTheApi(string? name, int descriptionLength)
    {
        // Arrange
        var client = new FakeRoleClient();
        var model = Create(client);
        var input = new RoleInput { Name = name, Description = new string('x', descriptionLength) };

        // Act
        await model.OnPostCreateAsync(input, TestContext.Current.CancellationToken);

        // Assert
        model.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        client.Sent.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateReportsSuccess()
    {
        // Arrange
        var client = new FakeRoleClient();
        var model = Create(client);

        // Act
        await model.OnPostUpdateAsync(Guid.NewGuid(), new RoleInput { Name = "Doctor" }, TestContext.Current.CancellationToken);

        // Assert
        model.Message.ShouldBe("Role updated.");
        client.Sent.ShouldNotBeNull().Name.ShouldBe("Doctor");
    }

    [Fact]
    public async Task UpdateRejectsATooLongNameWithoutCallingTheApi()
    {
        // Arrange
        var client = new FakeRoleClient();
        var model = Create(client);
        var input = new RoleInput { Name = new string('x', DataSchemeConstants.RoleNameMaxLength + 1) };

        // Act
        await model.OnPostUpdateAsync(Guid.NewGuid(), input, TestContext.Current.CancellationToken);

        // Assert
        model.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        client.Sent.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateShowsTheApiProblemWhenItRefuses()
    {
        // Arrange
        var model = Create(new FakeRoleClient
        {
            Write = () => ApiResult.Create<RoleResponse>(HttpStatusCode.Conflict, null, Problem("System roles cannot be renamed."))
        });

        // Act
        await model.OnPostUpdateAsync(Guid.NewGuid(), new RoleInput { Name = "Boss" }, TestContext.Current.CancellationToken);

        // Assert
        model.Message.ShouldBeNull();
        model.ErrorMessage.ShouldBe("System roles cannot be renamed.");
    }

    [Fact]
    public async Task DeleteRemovesTheRole()
    {
        // Arrange
        var client = new FakeRoleClient();
        var model = Create(client);
        var id = Guid.NewGuid();

        // Act
        await model.OnPostDeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        client.DeletedId.ShouldBe(id);
        model.Message.ShouldBe("Role deleted.");
    }

    [Fact]
    public async Task DeleteTellsNonAdministratorsTheyAreNotAllowed()
    {
        // Arrange
        var model = Create(new FakeRoleClient { Remove = () => ApiResult.Create(HttpStatusCode.Forbidden) });

        // Act
        await model.OnPostDeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        model.ErrorMessage.ShouldBe("Only administrators can change roles.");
        model.Message.ShouldBeNull();
    }

    [Fact]
    public async Task SendsTheTickedRights()
    {
        // Arrange
        var client = new FakeRoleClient();
        var model = Create(client);
        var input = new RoleInput { Name = "Nurse", Permissions = [Permissions.Users.View, Permissions.Roles.View] };

        // Act
        await model.OnPostUpdateAsync(Guid.NewGuid(), input, TestContext.Current.CancellationToken);

        // Assert
        client.Sent.ShouldNotBeNull().Permissions.ShouldBe([Permissions.Users.View, Permissions.Roles.View]);
    }

    [Fact]
    public async Task OnlyAdministratorsAreOfferedChanges()
    {
        // Arrange
        var administrator = Create(Listing(HttpStatusCode.OK));
        var other = Create(Listing(HttpStatusCode.OK), FakeCurrentUserProfile.With(Permissions.Roles.View));

        // Act
        await administrator.OnGetAsync(TestContext.Current.CancellationToken);
        await other.OnGetAsync(TestContext.Current.CancellationToken);

        // Assert
        administrator.CanManage.ShouldBeTrue();
        other.CanManage.ShouldBeFalse();
    }

    [Fact]
    public async Task SaysSoWhenThePersonMayNotViewRoles()
    {
        // Arrange
        var model = Create(Listing(HttpStatusCode.Forbidden), FakeCurrentUserProfile.With());

        // Act
        await model.OnGetAsync(TestContext.Current.CancellationToken);

        // Assert
        model.Forbidden.ShouldBeTrue();
        model.Unavailable.ShouldBeFalse();
    }
}
