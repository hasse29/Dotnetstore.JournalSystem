using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies.Images;

public sealed class OwnCompanyImageEndpointsTests
{
    private static List<RouteEndpoint> Map()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = "Server=localhost,1;Database=journalsystem;User Id=u;Password=p;TrustServerCertificate=True";
        builder.RegisterOrganizationServices();
        using var app = builder.Build();

        app.MapOwnCompanyImageEndpoints();

        return ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(x => x.Endpoints)
            .OfType<RouteEndpoint>()
            .ToList();
    }

    [Fact]
    public void MapOwnCompanyImageEndpointsRegistersAllRoutes()
    {
        // Arrange

        // Act
        var routes = Map()
            .Select(x => (Pattern: x.RoutePattern.RawText, Method: x.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single()))
            .ToList();

        // Assert
        routes.ShouldBe(
            [
                ("/api/organization/own-companies/{id:guid}/images", "GET"),
                ("/api/organization/own-companies/{id:guid}/images/{imageId:guid}", "GET"),
                ("/api/organization/own-companies/{id:guid}/logo", "PUT"),
                ("/api/organization/own-companies/{id:guid}/photos/{imageId:guid}", "PUT"),
                ("/api/organization/own-companies/{id:guid}/images/{imageId:guid}", "DELETE")
            ],
            ignoreOrder: true);
    }

    [Fact]
    public void OnlyTheUploadRoutesRaiseTheRequestSizeLimit()
    {
        // Arrange

        // Act
        var endpoints = Map();

        // Assert
        endpoints
            .Where(x => x.Metadata.GetMetadata<IRequestSizeLimitMetadata>() is not null)
            .Select(x => x.RoutePattern.RawText)
            .ShouldBe(
                [
                    "/api/organization/own-companies/{id:guid}/logo",
                    "/api/organization/own-companies/{id:guid}/photos/{imageId:guid}"
                ],
                ignoreOrder: true);
    }

    [Fact]
    public void MapOwnCompanyImageEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapOwnCompanyImageEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
