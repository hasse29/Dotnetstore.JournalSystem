using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;

public sealed class OwnCompanyEndpointsTests
{
    [Fact]
    public void MapOwnCompanyEndpointsRegistersAllRoutes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = "Server=localhost,1;Database=journalsystem;User Id=u;Password=p;TrustServerCertificate=True";
        builder.RegisterOrganizationServices();
        using var app = builder.Build();

        // Act
        app.MapOwnCompanyEndpoints();

        // Assert
        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(x => x.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(x => (Pattern: x.RoutePattern.RawText?.TrimEnd('/'), Method: x.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single()))
            .ToList();
        routes.ShouldBe(
            [
                ("/api/organization/own-companies", "GET"),
                ("/api/organization/own-companies/{id:guid}", "GET"),
                ("/api/organization/own-companies", "POST"),
                ("/api/organization/own-companies/{id:guid}", "PUT"),
                ("/api/organization/own-companies/{id:guid}", "DELETE")
            ],
            ignoreOrder: true);
    }

    [Fact]
    public void MapOwnCompanyEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapOwnCompanyEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
