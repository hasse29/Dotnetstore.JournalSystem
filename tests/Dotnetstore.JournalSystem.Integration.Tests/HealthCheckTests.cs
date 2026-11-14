using System.Net;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests;

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
}
