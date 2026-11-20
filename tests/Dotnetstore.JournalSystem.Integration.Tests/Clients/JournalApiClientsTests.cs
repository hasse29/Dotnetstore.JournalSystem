using System.Net;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Clients;

public sealed class JournalApiClientsTests(SqlServerFixture fixture)
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private sealed class MutableTokenProvider : IAccessTokenProvider
    {
        public string? Token { get; set; }

        public ValueTask<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(Token);
    }

    [Fact]
    public async Task RoleClientRunsTheFullCrudCycleAgainstTheHost()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token);
        await using var provider = host.CreateApiClients();
        var roles = provider.GetRequiredService<IRoleClient>();

        // Act
        var created = await roles.CreateAsync(new EditRoleRequest("Nurse", "Cares"), Token);
        var id = created.Value.ShouldNotBeNull().Id;
        var fetched = await roles.GetByIdAsync(id, Token);
        var updated = await roles.UpdateAsync(id, new EditRoleRequest("Senior nurse", "Cares more"), Token);
        var all = await roles.GetAllAsync(Token);
        var deleted = await roles.DeleteAsync(id, Token);
        var afterDelete = await roles.GetByIdAsync(id, Token);

        // Assert
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        fetched.Value.ShouldNotBeNull().Name.ShouldBe("Nurse");
        updated.Value.ShouldNotBeNull().Name.ShouldBe("Senior nurse");
        all.Value.ShouldNotBeNull().ShouldContain(x => x.Id == id);
        deleted.IsSuccess.ShouldBeTrue();
        afterDelete.IsNotFound.ShouldBeTrue();
    }

    [Fact]
    public async Task InvalidRequestComesBackAsFieldErrors()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token);
        await using var provider = host.CreateApiClients();
        var roles = provider.GetRequiredService<IRoleClient>();

        // Act
        var result = await roles.CreateAsync(new EditRoleRequest("", ""), Token);

        // Assert
        result.IsValidationError.ShouldBeTrue();
        result.Problem.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task UsersCallWithoutATokenIsUnauthorized()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await using var provider = host.CreateApiClients();
        var users = provider.GetRequiredService<IUserClient>();

        // Act
        var result = await users.GetAllAsync(Token);

        // Assert
        result.IsUnauthorized.ShouldBeTrue();
    }

    [Fact]
    public async Task LoginThenUsersCallSucceedsAndDuplicateUserConflicts()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        var tokens = new MutableTokenProvider();
        await using var provider = host.CreateApiClients(tokens);
        var auth = provider.GetRequiredService<IAuthClient>();
        var users = provider.GetRequiredService<IUserClient>();
        var roles = provider.GetRequiredService<IRoleClient>();
        var companies = provider.GetRequiredService<IOwnCompanyClient>();

        // Act
        var login = await auth.LoginAsync(new LoginRequest(ApiTestHost.AdminUsername, ApiTestHost.AdminPassword), Token);
        tokens.Token = login.Value.ShouldNotBeNull().AccessToken;
        var companyId = (await companies.GetAllAsync(Token)).Value.ShouldNotBeNull().Single().Id;
        var roleId = (await roles.GetAllAsync(Token)).Value.ShouldNotBeNull()
            .Single(x => x.Name == RoleNames.Administrator).Id;
        var request = new CreateUserRequest(
            "Anna", null, "Svensson", "anna@example.com", companyId, roleId, AuthProvider.Local, "a-long-enough-password");
        var first = await users.CreateAsync(request, Token);
        var second = await users.CreateAsync(request, Token);
        var list = await users.GetAllAsync(Token);

        // Assert
        first.IsSuccess.ShouldBeTrue();
        second.IsConflict.ShouldBeTrue();
        list.Value.ShouldNotBeNull().ShouldContain(x => x.Username == "anna@example.com");
    }

    [Fact]
    public async Task LoginWithWrongPasswordIsUnauthorized()
    {
        // Arrange
        await using var host = await ApiTestHost.CreateAsync(fixture, Token, withBootstrapAdmin: true);
        await using var provider = host.CreateApiClients();
        var auth = provider.GetRequiredService<IAuthClient>();

        // Act
        var result = await auth.LoginAsync(new LoginRequest(ApiTestHost.AdminUsername, "wrong-password-123"), Token);

        // Assert
        result.IsUnauthorized.ShouldBeTrue();
    }
}
