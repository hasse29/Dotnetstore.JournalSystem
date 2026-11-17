using System.Net;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests.Extensions;

public sealed class HealthCheckTests
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
    public async Task HealthEndpointStaysHealthyWhenDatabaseIsUnavailable()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:journalsystem"] = "Server=localhost,1;Database=unreachable;User Id=u;Password=p;Connect Timeout=1;TrustServerCertificate=True"
        };
        await using var factory = TestHost.Create("Production", settings);
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
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
    public async Task ReadyEndpointReportsUnhealthyWhenDatabaseIsUnavailable()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:journalsystem"] = "Server=localhost,1;Database=unreachable;User Id=u;Password=p;Connect Timeout=1;TrustServerCertificate=True"
        };
        await using var factory = TestHost.Create("Development", settings);
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("/ready", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }
}
