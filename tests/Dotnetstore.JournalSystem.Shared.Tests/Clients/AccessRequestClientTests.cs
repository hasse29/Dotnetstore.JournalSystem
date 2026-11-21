using System.Net;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Shared.Tests.Clients;

public sealed class AccessRequestClientTests
{
    private static readonly Guid Id = Guid.Parse("11111111-2222-3333-4444-555555555555");

    private static string Body(string status = "Pending") =>
        $$"""{"id":"{{Id}}","displayName":"Anna Svensson","username":"anna@example.com","status":{{(int)Enum.Parse<AccessRequestStatus>(status)}},"requestedAt":"2026-11-21T10:00:00Z","decidedAt":null,"decisionNote":null}""";

    private static (StubHandler Handler, ServiceProvider Provider, IAccessRequestClient Client) Create(HttpStatusCode status, string body)
    {
        var handler = new StubHandler(_ => StubHandler.Json(status, body));
        var provider = ClientsTestHost.Build(handler);
        return (handler, provider, provider.GetRequiredService<IAccessRequestClient>());
    }

    [Fact]
    public async Task RequestMyAccessPostsToTheMeRouteAndReadsTheStatus()
    {
        // Arrange
        var (handler, provider, client) = Create(HttpStatusCode.OK, Body());
        await using var _ = provider;
        using var __ = handler;

        // Act
        var result = await client.RequestMyAccessAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().Status.ShouldBe(AccessRequestStatus.Pending);
        handler.Requests.Single().Method.ShouldBe(HttpMethod.Post);
        handler.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/api/organization/access-requests/me");
        handler.Bodies.Single().ShouldBeNull();
    }

    [Fact]
    public async Task GetMineUsesGetOnTheSameRoute()
    {
        // Arrange
        var (handler, provider, client) = Create(HttpStatusCode.OK, Body("Rejected"));
        await using var _ = provider;
        using var __ = handler;

        // Act
        var result = await client.GetMineAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Value.ShouldNotBeNull().Status.ShouldBe(AccessRequestStatus.Rejected);
        handler.Requests.Single().Method.ShouldBe(HttpMethod.Get);
        handler.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/api/organization/access-requests/me");
    }

    [Fact]
    public async Task GetMineReportsNotFoundAndForbiddenWithoutThrowing()
    {
        // Arrange
        var (handler, provider, client) = Create(HttpStatusCode.Forbidden, string.Empty);
        await using var _ = provider;
        using var __ = handler;

        // Act
        var result = await client.GetMineAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsForbidden.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Theory]
    [InlineData(null, "/api/organization/access-requests")]
    [InlineData(AccessRequestStatus.Pending, "/api/organization/access-requests?status=Pending")]
    [InlineData(AccessRequestStatus.Rejected, "/api/organization/access-requests?status=Rejected")]
    public async Task GetAllAddsTheStatusFilterOnlyWhenOneIsGiven(AccessRequestStatus? status, string expected)
    {
        // Arrange
        var (handler, provider, client) = Create(HttpStatusCode.OK, "[" + Body() + "]");
        await using var _ = provider;
        using var __ = handler;

        // Act
        var result = await client.GetAllAsync(status, TestContext.Current.CancellationToken);

        // Assert
        result.Value.ShouldNotBeNull().Length.ShouldBe(1);
        handler.Requests.Single().RequestUri!.PathAndQuery.ShouldBe(expected);
    }

    [Fact]
    public async Task ApproveSendsTheChosenRoleAndCompanyToTheRequestsRoute()
    {
        // Arrange
        var (handler, provider, client) = Create(HttpStatusCode.OK, Body("Approved"));
        await using var _ = provider;
        using var __ = handler;
        var company = Guid.NewGuid();
        var role = Guid.NewGuid();

        // Act
        var result = await client.ApproveAsync(Id, new ApproveAccessRequestRequest(company, role, "Anna", null, "Svensson", false), TestContext.Current.CancellationToken);

        // Assert
        result.Value.ShouldNotBeNull().Status.ShouldBe(AccessRequestStatus.Approved);
        handler.Requests.Single().RequestUri!.AbsolutePath.ShouldBe($"/api/organization/access-requests/{Id}/approve");
        handler.Bodies.Single().ShouldNotBeNull().ShouldContain(company.ToString());
        handler.Bodies.Single()!.ShouldContain(role.ToString());
    }

    [Fact]
    public async Task RejectSendsTheNote()
    {
        // Arrange
        var (handler, provider, client) = Create(HttpStatusCode.OK, Body("Rejected"));
        await using var _ = provider;
        using var __ = handler;

        // Act
        await client.RejectAsync(Id, new RejectAccessRequestRequest("Not an employee"), TestContext.Current.CancellationToken);

        // Assert
        handler.Requests.Single().RequestUri!.AbsolutePath.ShouldBe($"/api/organization/access-requests/{Id}/reject");
        handler.Bodies.Single().ShouldNotBeNull().ShouldContain("Not an employee");
    }

    [Fact]
    public async Task ReopenPostsWithoutABody()
    {
        // Arrange
        var (handler, provider, client) = Create(HttpStatusCode.OK, Body());
        await using var _ = provider;
        using var __ = handler;

        // Act
        await client.ReopenAsync(Id, TestContext.Current.CancellationToken);

        // Assert
        handler.Requests.Single().Method.ShouldBe(HttpMethod.Post);
        handler.Requests.Single().RequestUri!.AbsolutePath.ShouldBe($"/api/organization/access-requests/{Id}/reopen");
        handler.Bodies.Single().ShouldBeNull();
    }

    [Fact]
    public async Task ApproveReportsAConflictWithTheApisReason()
    {
        // Arrange
        var (handler, provider, client) = Create(HttpStatusCode.Conflict, "\"Only a pending request can be approved.\"");
        await using var _ = provider;
        using var __ = handler;

        // Act
        var result = await client.ApproveAsync(Id, new ApproveAccessRequestRequest(Guid.NewGuid(), Guid.NewGuid(), "Anna", null, "Svensson", false), TestContext.Current.CancellationToken);

        // Assert
        result.IsConflict.ShouldBeTrue();
        result.Problem.Detail.ShouldBe("Only a pending request can be approved.");
    }
}
