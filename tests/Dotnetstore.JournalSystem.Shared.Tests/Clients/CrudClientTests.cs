using System.Net;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Shared.Tests.Clients;

public sealed class CrudClientTests
{
    private static readonly Guid Id = Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Fact]
    public async Task GetAllReturnsTheListOnSuccess()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.OK,
            $$"""[{"id":"{{Id}}","name":"Admin","description":"d"}]"""));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        var result = await client.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull().Single().Name.ShouldBe("Admin");
        handler.Requests.Single().Method.ShouldBe(HttpMethod.Get);
        handler.Requests.Single().RequestUri.ShouldBe(new Uri("https://api.test/api/organization/roles"));
    }

    [Fact]
    public async Task GetByIdReplacesTheIdTemplateInTheRoute()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.OK,
            $$"""{"id":"{{Id}}","name":"Admin","description":"d"}"""));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        var result = await client.GetByIdAsync(Id, TestContext.Current.CancellationToken);

        // Assert
        result.Value.ShouldNotBeNull().Id.ShouldBe(Id);
        handler.Requests.Single().RequestUri!.AbsolutePath.ShouldBe($"/api/organization/roles/{Id}");
    }

    [Fact]
    public async Task GetByIdReturnsNotFoundWithoutThrowing()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Empty(HttpStatusCode.NotFound));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        var result = await client.GetByIdAsync(Id, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsNotFound.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }

    [Fact]
    public async Task CreatePostsCamelCaseJson()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.Created,
            $$"""{"id":"{{Id}}","name":"Nurse","description":"d"}"""));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        var result = await client.CreateAsync(new EditRoleRequest("Nurse", "d"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.StatusCode.ShouldBe(HttpStatusCode.Created);
        handler.Requests.Single().Method.ShouldBe(HttpMethod.Post);
        handler.Bodies.Single().ShouldBe("""{"name":"Nurse","description":"d","permissions":null}""");
    }

    [Fact]
    public async Task UpdatePutsToTheItemRoute()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.OK,
            $$"""{"id":"{{Id}}","name":"N","description":"d"}"""));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        var result = await client.UpdateAsync(Id, new EditRoleRequest("N", "d"), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        handler.Requests.Single().Method.ShouldBe(HttpMethod.Put);
        handler.Requests.Single().RequestUri!.AbsolutePath.ShouldBe($"/api/organization/roles/{Id}");
    }

    [Fact]
    public async Task DeleteSucceedsOnNoContent()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Empty(HttpStatusCode.NoContent));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        var result = await client.DeleteAsync(Id, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        handler.Requests.Single().Method.ShouldBe(HttpMethod.Delete);
    }

    [Fact]
    public async Task ValidationProblemIsParsedIntoFieldErrors()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.BadRequest,
            """{"title":"One or more validation errors occurred.","errors":{"Name":["Name is required."]}}""",
            "application/problem+json"));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        var result = await client.CreateAsync(new EditRoleRequest("", "d"), TestContext.Current.CancellationToken);

        // Assert
        result.IsValidationError.ShouldBeTrue();
        result.Problem.Errors["Name"].ShouldBe(["Name is required."]);
    }

    [Fact]
    public async Task ConflictPlainStringBodyBecomesTheProblemDetail()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.Conflict, "\"Role already exists.\""));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        var result = await client.CreateAsync(new EditRoleRequest("Admin", "d"), TestContext.Current.CancellationToken);

        // Assert
        result.IsConflict.ShouldBeTrue();
        result.IsValidationError.ShouldBeFalse();
        result.Problem.Detail.ShouldBe("Role already exists.");
    }

    [Fact]
    public async Task NonJsonErrorBodyIsKeptAsDetail()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Json(HttpStatusCode.ServiceUnavailable, "down", "text/plain"));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        var result = await client.GetAllAsync(TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        result.Problem.Detail.ShouldBe("down");
    }

    [Fact]
    public async Task CreateRejectsANullRequest()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Empty(HttpStatusCode.OK));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IRoleClient>();

        // Act
        var act = () => client.CreateAsync(null!, TestContext.Current.CancellationToken);

        // Assert
        await Should.ThrowAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task UserClientCreatesAtTheUsersRoute()
    {
        // Arrange
        using var handler = new StubHandler(_ => StubHandler.Empty(HttpStatusCode.BadRequest));
        await using var provider = ClientsTestHost.Build(handler);
        var client = provider.GetRequiredService<IUserClient>();

        // Act
        await client.CreateAsync(
            new CreateUserRequest("A", null, "B", "a@b.se", Guid.Empty, Guid.Empty, AuthProvider.Local, "pw"),
            TestContext.Current.CancellationToken);

        // Assert
        handler.Requests.Single().RequestUri!.AbsolutePath.ShouldBe("/api/organization/users");
    }
}
