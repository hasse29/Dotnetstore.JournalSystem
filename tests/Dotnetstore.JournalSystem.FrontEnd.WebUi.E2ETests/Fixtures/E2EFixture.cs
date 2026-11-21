using System.Security.Claims;
using Dotnetstore.Security.Handlers;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dotnetstore.JournalSystem.Api.Contact.Extensions;
using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Extensions;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Shouldly;
using Testcontainers.MsSql;

[assembly: AssemblyFixture(typeof(E2EFixture))]

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;

/// <summary>
/// One real stack for the whole run: SQL Server in a container, the API (in-memory test server), the WebUi on a real
/// Kestrel socket so a browser can reach it, and a headless Chromium driven by Playwright.
/// </summary>
public sealed class E2EFixture : IAsyncLifetime
{
    public const string AdminUsername = "admin@example.com";
    public const string AdminFullName = "System Administrator";
    public const string UserFullName = "Test Person";
    public const string AdminPassword = "correct-horse-battery-staple";
    public const string UserPassword = "a-long-enough-password";

    private const string EncryptionKeyId = "test";
    private const string EncryptionKey = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=";
    private const string TokenSigningKeyId = "k1";
    private const string TokenSigningKey = "ZGV2LW9ubHktdGVzdC1zaWduaW5nLWtleS0zMi1ieXRlcyE=";

    private readonly MsSqlContainer _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
    private WebApplicationFactory<Host.Program>? _api;
    private WebApplication? _webUi;
    private FakeIdentityProvider? _idp;
    private IPlaywright? _playwright;

    public string WebUiAddress { get; private set; } = string.Empty;

    public IBrowser Browser { get; private set; } = null!;

    internal FakeIdentityProvider Idp => _idp!;

    public async ValueTask InitializeAsync()
    {
        await _sql.StartAsync();

        var connectionString = new SqlConnectionStringBuilder(_sql.GetConnectionString())
        {
            InitialCatalog = "e2e_" + Guid.NewGuid().ToString("N")
        }.ConnectionString;

        _api = CreateApi(connectionString);
        await _api.Services.InitializeOrganizationDatabaseAsync(CancellationToken.None);
        await _api.Services.InitializeContactDatabaseAsync(CancellationToken.None);
        await SeedBootstrapAdminAsync(_api);

        _idp = await FakeIdentityProvider.StartAsync(MintAdminAccessTokenAsync, MintPersonAccessTokenAsync);
        _webUi = await StartWebUiAsync(_api, _idp);
        WebUiAddress = _webUi.Services
            .GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!
            .Addresses.First()
            .TrimEnd('/');

        Microsoft.Playwright.Program.Main(["install", "chromium"]).ShouldBe(0);

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Environment.GetEnvironmentVariable("E2E_HEADED") != "1"
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        _playwright?.Dispose();

        if (_idp is not null)
        {
            await _idp.DisposeAsync();
        }

        if (_webUi is not null)
        {
            await _webUi.DisposeAsync();
        }

        if (_api is not null)
        {
            await _api.DisposeAsync();
        }

        await _sql.DisposeAsync();
    }

    // Entra access tokens carry the person's object id and no application claims; the API validates this one as its own token.
    private Task<string> MintPersonAccessTokenAsync(FakePerson person)
    {
        var tokenHandler = _api!.Services.GetRequiredService<ITokenHandler>();
        var token = tokenHandler.CreateAccessToken(
            person.ObjectId,
            [new Claim("oid", person.ObjectId), new Claim("name", person.DisplayName), new Claim("preferred_username", person.UserName)]);
        return Task.FromResult(token.Value);
    }

    private async Task<string> MintAdminAccessTokenAsync()
    {
        using var client = _api!.CreateClient();
        using var login = await client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Auth.Login, UriKind.Relative),
            new LoginRequest(AdminUsername, AdminPassword));
        return (await login.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
    }

    public async Task<IBrowserContext> NewContextAsync(ViewportSize? viewport = null) =>
        await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = WebUiAddress,
            ViewportSize = viewport ?? new ViewportSize { Width = 1280, Height = 800 }
        });

    /// <summary>Creates a user through the API as the bootstrap administrator and returns the username.</summary>
    public async Task<string> CreateUserAsync(string roleName, string? username = null)
    {
        username ??= $"{roleName.ToUpperInvariant()}-{Guid.NewGuid():N}@example.com";

        using var client = _api!.CreateClient();
        using var login = await client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Auth.Login, UriKind.Relative),
            new LoginRequest(AdminUsername, AdminPassword));
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var companies = await client.GetFromJsonAsync<OwnCompanyResponse[]>(
            new Uri(ApiEndpoints.Organization.OwnCompanies.GetAll, UriKind.Relative));

        using var roleResponse = await client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Organization.Roles.Create, UriKind.Relative),
            new EditRoleRequest(roleName + "-" + Guid.NewGuid().ToString("N")[..6], "E2E role"));
        roleResponse.EnsureSuccessStatusCode();
        var role = (await roleResponse.Content.ReadFromJsonAsync<RoleResponse>())!;

        using var userResponse = await client.PostAsJsonAsync(
            new Uri(ApiEndpoints.Organization.Users.Create, UriKind.Relative),
            new CreateUserRequest("Test", null, "Person", username, companies![0].Id, role.Id, AuthProvider.Local, UserPassword));
        userResponse.EnsureSuccessStatusCode();

        return username;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000", Justification = "The derived factory is owned and disposed by the fixture.")]
    private static WebApplicationFactory<Host.Program> CreateApi(string connectionString)
    {
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:" + Dotnetstore.JournalSystem.Api.Organization.Extensions.HostApplicationBuilderExtensions.DatabaseConnectionName] = connectionString,
            ["Encryption:Keys:" + EncryptionKeyId] = EncryptionKey,
            ["Encryption:CurrentKeyId"] = EncryptionKeyId,
            ["Tokens:SigningKeys:" + TokenSigningKeyId] = TokenSigningKey,
            ["Bootstrap:Admin:Username"] = AdminUsername,
            ["Bootstrap:Admin:Password"] = AdminPassword,
            ["RateLimiting:LoginPermitLimit"] = "10000",
            ["RateLimiting:PermitLimit"] = "10000"
        };

        return new WebApplicationFactory<Host.Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            foreach (var (key, value) in settings)
            {
                builder.UseSetting(key, value);
            }
        });
    }

    private static async Task<WebApplication> StartWebUiAsync(WebApplicationFactory<Host.Program> api, FakeIdentityProvider idp)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(WebApplicationExtensions).Assembly.GetName().Name,
            ContentRootPath = FindWebUiProjectDirectory(),
            EnvironmentName = "Production"
        });

        StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration["JournalApi:BaseAddress"] = "http://localhost";
        builder.Configuration["Authentication:Entra:Enabled"] = "true";
        builder.Configuration["AzureAd:Authority"] = idp.Authority.ToString();
        builder.Configuration["AzureAd:ClientId"] = FakeIdentityProvider.ClientId;
        builder.Configuration["AzureAd:ClientCredentials:0:ClientSecret"] = FakeIdentityProvider.ClientSecret;
        builder.RegisterServices();
        builder.Services.ConfigureHttpClientDefaults(http =>
            http.ConfigurePrimaryHttpMessageHandler(() => api.Server.CreateHandler()));

        var app = builder.Build().RegisterMiddlewares();
        await app.StartAsync();
        return app;
    }

    private static string FindWebUiProjectDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Dotnetstore.JournalSystem.FrontEnd.WebUi");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find the WebUi project directory above the test output.");
    }

    private static async Task SeedBootstrapAdminAsync(WebApplicationFactory<Host.Program> api)
    {
        var seedPath = Path.Combine(Path.GetTempPath(), $"journalsystem-seed-{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(seedPath, """{"ownCompanies":[{"name":"Test AB"}]}""");

        try
        {
            await api.Services.SeedOrganizationDataAsync([seedPath], CancellationToken.None);
        }
        finally
        {
            File.Delete(seedPath);
        }

        await api.Services.BootstrapOrganizationAdminAsync(
            api.Services.GetRequiredService<IConfiguration>(),
            CancellationToken.None);
    }
}
