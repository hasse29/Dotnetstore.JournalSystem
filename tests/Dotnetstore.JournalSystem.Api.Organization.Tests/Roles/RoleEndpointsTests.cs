using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;

public sealed class RoleEndpointsTests
{
    [Fact]
    public void MapRoleEndpointsRegistersAllRoutes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = "Server=localhost,1;Database=journalsystem;User Id=u;Password=p;TrustServerCertificate=True";
        builder.RegisterOrganizationServices();
        using var app = builder.Build();

        // Act
        app.MapRoleEndpoints();

        // Assert
        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(x => x.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(x => (Pattern: x.RoutePattern.RawText?.TrimEnd('/'), Method: x.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single()))
            .ToList();
        routes.ShouldBe(
            [
                ("/api/organization/roles", "GET"),
                ("/api/organization/roles/{id:guid}", "GET"),
                ("/api/organization/roles", "POST"),
                ("/api/organization/roles/{id:guid}", "PUT"),
                ("/api/organization/roles/{id:guid}", "DELETE")
            ],
            ignoreOrder: true);
    }

    [Fact]
    public void MapRoleEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapRoleEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
