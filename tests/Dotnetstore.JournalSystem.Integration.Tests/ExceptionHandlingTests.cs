using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dotnetstore.JournalSystem.Integration.Tests;

public sealed class ExceptionHandlingTests
{
    private const string Secret = "super-secret-internal-detail";

    [Fact]
    public async Task UnhandledExceptionReturnsProblemDetailsWithoutInternals()
    {
        // Arrange
        await using var factory = TestHost.Create(
            "Production",
            configureServices: services => services.AddSingleton<IStartupFilter, ThrowingEndpointStartupFilter>());
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("/boom", UriKind.Relative), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        body.ShouldNotContain(Secret);
        body.ShouldNotContain("InvalidOperationException");
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("status").GetInt32().ShouldBe(500);
    }

    [Fact]
    public async Task UnknownRouteReturnsProblemDetails()
    {
        // Arrange
        await using var factory = TestHost.Create("Production");
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync(new Uri("/does-not-exist", UriKind.Relative), TestContext.Current.CancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        using var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("status").GetInt32().ShouldBe(404);
    }

    private sealed class ThrowingEndpointStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                next(app);
                app.Map("/boom", branch => branch.Run(_ => throw new InvalidOperationException(Secret)));
            };
    }
}
