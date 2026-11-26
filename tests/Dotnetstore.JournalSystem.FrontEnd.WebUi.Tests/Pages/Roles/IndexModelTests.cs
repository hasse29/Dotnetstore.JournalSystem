using System.Net;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Roles;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
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
        var model = new IndexModel(Listing(HttpStatusCode.OK, "Porter", "doctor", "Nurse"));

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
        var model = new IndexModel(Listing(HttpStatusCode.InternalServerError));

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
        var model = new IndexModel(new FakeRoleClient { List = () => throw new HttpRequestException("down") });

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
        var model = new IndexModel(client);

        // Act
        var result = await model.OnPostCreateAsync(new RoleInput { Name = "  Nurse ", Description = " Care " }, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeOfType<RedirectToPageResult>();
        model.Message.ShouldBe("Role created.");
        client.Sent.ShouldBe(new EditRoleRequest("Nurse", "Care"));
    }

    [Fact]
    public async Task CreateSendsAnEmptyDescriptionWhenNoneIsGiven()
    {
        // Arrange
        var client = new FakeRoleClient();
        var model = new IndexModel(client);

        // Act
        await model.OnPostCreateAsync(new RoleInput { Name = "Nurse" }, TestContext.Current.CancellationToken);

        // Assert
        client.Sent.ShouldBe(new EditRoleRequest("Nurse", string.Empty));
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData("  ", 0)]
    [InlineData("Nurse", DataSchemeConstants.RoleDescriptionMaxLength + 1)]
    public async Task CreateRejectsInvalidInputWithoutCallingTheApi(string? name, int descriptionLength)
    {
        // Arrange
        var client = new FakeRoleClient();
        var model = new IndexModel(client);
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
        var model = new IndexModel(client);

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
        var model = new IndexModel(client);
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
        var model = new IndexModel(new FakeRoleClient
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
        var model = new IndexModel(client);
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
        var model = new IndexModel(new FakeRoleClient { Remove = () => ApiResult.Create(HttpStatusCode.Forbidden) });

        // Act
        await model.OnPostDeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        model.ErrorMessage.ShouldBe("Only administrators can change roles.");
        model.Message.ShouldBeNull();
    }
}
