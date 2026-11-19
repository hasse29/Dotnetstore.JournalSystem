using Dotnetstore.JournalSystem.Api.Contact.Extensions;
using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Addresses;

public sealed class AddressEndpointsTests
{
    [Fact]
    public void MapAddressEndpointsRegistersAllRoutes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = "Server=localhost,1;Database=journalsystem;User Id=u;Password=p;TrustServerCertificate=True";
        builder.RegisterContactServices();
        using var app = builder.Build();

        // Act
        app.MapAddressEndpoints();

        // Assert
        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(x => x.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(x => (Pattern: x.RoutePattern.RawText?.TrimEnd('/'), Method: x.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single()))
            .ToList();
        routes.ShouldBe(
            [
                ("/api/contact/addresses", "GET"),
                ("/api/contact/addresses/{id:guid}", "GET"),
                ("/api/contact/addresses", "POST"),
                ("/api/contact/addresses/{id:guid}", "PUT"),
                ("/api/contact/addresses/{id:guid}", "DELETE")
            ],
            ignoreOrder: true);
    }

    [Fact]
    public void MapAddressEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapAddressEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
