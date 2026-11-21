using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Users;

public sealed class UserEndpointsTests
{
    [Fact]
    public void MapUserEndpointsRegistersAllRoutes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = "Server=localhost,1;Database=journalsystem;User Id=u;Password=p;TrustServerCertificate=True";
        builder.RegisterOrganizationServices();
        using var app = builder.Build();

        // Act
        app.MapUserEndpoints();

        // Assert
        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(x => x.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(x => (Pattern: x.RoutePattern.RawText?.TrimEnd('/'), Method: x.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single()))
            .ToList();
        routes.ShouldBe(
            [
                ("/api/organization/users", "GET"),
                ("/api/organization/users/{id:guid}", "GET"),
                ("/api/organization/users", "POST"),
                ("/api/organization/users/{id:guid}", "PUT"),
                ("/api/organization/users/{id:guid}", "DELETE"),
                ("/api/organization/users/me", "GET")
            ],
            ignoreOrder: true);
    }

    [Fact]
    public void MapUserEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapUserEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
