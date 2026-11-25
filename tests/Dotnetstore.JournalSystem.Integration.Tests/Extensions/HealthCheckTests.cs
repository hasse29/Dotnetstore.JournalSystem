using System.Net;
using Dotnetstore.JournalSystem.Integration.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Extensions;

public sealed class HealthCheckTests(SqlServerFixture fixture)
{
    [Fact]
    public async Task HealthEndpointReturnsHealthyWithoutDetails()
    {
        // Arrange
        await using var factory = TestHost.Create("Production");
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        body.ShouldBe("Healthy");
    }

    [Fact]
    public async Task HealthEndpointStaysHealthyWhenTheDatabaseGoesAway()
    {
        // Arrange
        var connectionString = await ProvisionDatabaseAsync();
        await using var factory = TestHost.Create("Production", Settings(connectionString));
        using var client = factory.CreateClient();
        await TakeOfflineAsync(connectionString);

        // Act
        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public void TheHostDoesNotStartWhenTheDatabaseIsUnavailable()
    {
        // Arrange
        using var factory = TestHost.Create(
            "Production",
            Settings("Server=localhost,1;Database=unreachable;User Id=u;Password=p;Connect Timeout=1;TrustServerCertificate=True"));

        // Act
        var action = () => factory.CreateClient();

        // Assert
        action.ShouldThrow<Exception>();
    }

    [Fact]
    public async Task ReadyEndpointIsNotExposedInProduction()
    {
        // Arrange
        await using var factory = TestHost.Create("Production");
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("/ready", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReadyEndpointIsExposedInDevelopment()
    {
        // Arrange
        await using var factory = TestHost.Create("Development");
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("/ready", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldNotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReadyEndpointReportsUnhealthyWhenTheDatabaseGoesAway()
    {
        // Arrange
        var connectionString = await ProvisionDatabaseAsync();
        await using var factory = TestHost.Create("Development", Settings(connectionString));
        using var client = factory.CreateClient();
        await TakeOfflineAsync(connectionString);

        // Act
        using var response = await client.GetAsync(new Uri("/ready", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    private static Dictionary<string, string?> Settings(string connectionString) =>
        new() { ["ConnectionStrings:journalsystem"] = connectionString };

    private async Task<string> ProvisionDatabaseAsync()
    {
        var connectionString = fixture.CreateDatabaseConnectionString();
        await TestDatabase.ProvisionAsync(connectionString, TestContext.Current.CancellationToken);
        return connectionString;
    }

    private static Task TakeOfflineAsync(string connectionString) =>
        TestDatabase.ExecuteOnMasterAsync(
            connectionString,
            "ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE;",
            TestContext.Current.CancellationToken);
}
