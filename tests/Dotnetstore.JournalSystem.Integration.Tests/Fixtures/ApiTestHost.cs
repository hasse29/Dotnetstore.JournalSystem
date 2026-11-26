using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Dotnetstore.Security.Handlers;
using Dotnetstore.JournalSystem.Api.Contact.Extensions;
using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using OrganizationExtensions = Dotnetstore.JournalSystem.Api.Organization.Extensions.HostApplicationBuilderExtensions;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetstore.JournalSystem.Integration.Tests.Fixtures;

internal sealed class ApiTestHost : IAsyncDisposable
{
    internal const string AdminUsername = "admin@example.com";
    internal const string AdminPassword = "correct-horse-battery-staple";

    private readonly WebApplicationFactory<Host.Program> _factory;
    private readonly string _connectionString;

    private ApiTestHost(WebApplicationFactory<Host.Program> factory, HttpClient client, string connectionString)
    {
        _factory = factory;
        Client = client;
        _connectionString = connectionString;
    }

    internal HttpClient Client { get; }

    internal static async Task<ApiTestHost> CreateAsync(
        SqlServerFixture fixture,
        CancellationToken cancellationToken,
        bool withBootstrapAdmin = false)
    {
        var connectionString = fixture.CreateDatabaseConnectionString();
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:" + OrganizationExtensions.DatabaseConnectionName] = connectionString,
            // Tests that wait for background work poll the API; rate limiting has its own tests.
            ["RateLimiting:PermitLimit"] = "10000"
        };

        if (withBootstrapAdmin)
        {
            settings["Bootstrap:Admin:Username"] = AdminUsername;
            settings["Bootstrap:Admin:Password"] = AdminPassword;
        }

        await TestDatabase.ProvisionAsync(connectionString, cancellationToken);
        var factory = TestHost.Create("Production", settings);
        await factory.Services.InitializeOrganizationDatabaseAsync(cancellationToken);
        await factory.Services.InitializeContactDatabaseAsync(cancellationToken);

        if (withBootstrapAdmin)
        {
            await SeedBootstrapAdminAsync(factory, cancellationToken);
        }

        var client = factory.CreateClient();

        return new ApiTestHost(factory, client, connectionString);
    }

    internal static async Task<ApiTestHost> CreateSignedInAsync(
        SqlServerFixture fixture,
        CancellationToken cancellationToken)
    {
        var host = await CreateAsync(fixture, cancellationToken, withBootstrapAdmin: true);
        await host.SignInAsAdminAsync(cancellationToken);
        return host;
    }

    /// <summary>What a role could do before rights existed, and what existing roles were given.</summary>
    internal static readonly IReadOnlyList<string> ViewRights =
    [
        Permissions.OwnCompanies.View,
        Permissions.Roles.View,
        Permissions.WorkGroups.View,
        Permissions.ContactDetails.View,
        Permissions.ContactTypes.View
    ];

    /// <summary>
    /// Creates a user without the administrator role and signs in as them. Their role holds <see cref="ViewRights"/>
    /// unless other rights are given. Requires an administrator session.
    /// </summary>
    internal async Task SignInAsNewNonAdminAsync(
        CancellationToken cancellationToken,
        IReadOnlyList<string>? permissions = null)
    {
        const string username = "nurse@example.com";
        const string password = "a-long-enough-password";

        var companies = await Client.GetFromJsonAsync<OwnCompanyResponse[]>(
            new Uri(ApiEndpoints.Organization.OwnCompanies.GetAll, UriKind.Relative),
            cancellationToken);
        using var role = await Client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Organization.Roles.Create, UriKind.Relative),
            new EditRoleRequest("Nurse", "Description", permissions ?? ViewRights),
            cancellationToken);
        role.EnsureSuccessStatusCode();
        var roleId = (await role.Content.ReadFromJsonAsync<RoleResponse>(cancellationToken))!.Id;
        using var user = await Client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Organization.Users.Create, UriKind.Relative),
            new CreateUserRequest("Test", null, "Nurse", username, companies![0].Id, roleId, AuthProvider.Local, password),
            cancellationToken);
        user.EnsureSuccessStatusCode();

        using var response = await Client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Auth.Login, UriKind.Relative),
            new LoginRequest(username, password),
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
    }

    /// <summary>
    /// Signs in the way a Microsoft account does: a valid token that names the person's object id and carries no
    /// application claims. Until an administrator approves them they have no account.
    /// </summary>
    internal void SignInAsMicrosoftPerson(string objectId)
    {
        var token = _factory.Services.GetRequiredService<ITokenHandler>().CreateAccessToken(
            objectId,
            [new Claim("oid", objectId), new Claim("name", "Maria Microsoft"), new Claim("preferred_username", "maria@example.com")]);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
    }

    internal ServiceProvider CreateApiClients(IAccessTokenProvider? tokenProvider = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["JournalApi:BaseAddress"] = "http://localhost" })
            .Build();

        var services = new ServiceCollection();
        if (tokenProvider is not null)
        {
            services.AddSingleton(tokenProvider);
        }

        services.AddJournalApiClients(configuration);
        services.ConfigureHttpClientDefaults(builder =>
            builder.ConfigurePrimaryHttpMessageHandler(() => _factory.Server.CreateHandler()));

        return services.BuildServiceProvider(validateScopes: true);
    }

    internal async Task SignInAsAdminAsync(CancellationToken cancellationToken)
    {
        using var response = await Client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Auth.Login, UriKind.Relative),
            new LoginRequest(AdminUsername, AdminPassword),
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100", Justification = "Test-only helper; callers pass fixed SQL literals.")]
    internal async Task<string?> ReadStoredTextAsync(string columnSql, Guid id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = columnSql;
        command.Parameters.AddWithValue("@id", id);
        return (await command.ExecuteScalarAsync()) as string;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100", Justification = "The database name is generated by the test fixture from a GUID.")]
    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();

        await using var connection = new SqlConnection(new SqlConnectionStringBuilder(_connectionString) { InitialCatalog = "master" }.ConnectionString);
        await connection.OpenAsync();
        var databaseName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;
        await using var command = connection.CreateCommand();
        command.CommandText = $"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{databaseName}];";
        await command.ExecuteNonQueryAsync();
    }

    private static async Task SeedBootstrapAdminAsync(
        WebApplicationFactory<Host.Program> factory,
        CancellationToken cancellationToken)
    {
        var seedPath = Path.Combine(Path.GetTempPath(), $"journalsystem-seed-{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(seedPath, """{"ownCompanies":[{"name":"Test AB"}]}""", cancellationToken);

        try
        {
            await factory.Services.SeedOrganizationDataAsync([seedPath], cancellationToken);
        }
        finally
        {
            File.Delete(seedPath);
        }

        await factory.Services.BootstrapOrganizationAdminAsync(
            factory.Services.GetRequiredService<IConfiguration>(),
            cancellationToken);
    }
}
