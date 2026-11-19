using Dotnetstore.JournalSystem.Api.Contact.Extensions;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.AddressTypes;

public sealed class AddressTypeEndpointsTests
{
    [Fact]
    public void MapAddressTypeEndpointsRegistersAllRoutes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = "Server=localhost,1;Database=journalsystem;User Id=u;Password=p;TrustServerCertificate=True";
        builder.RegisterContactServices();
        using var app = builder.Build();

        // Act
        app.MapAddressTypeEndpoints();

        // Assert
        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(x => x.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(x => (Pattern: x.RoutePattern.RawText?.TrimEnd('/'), Method: x.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single()))
            .ToList();
        routes.ShouldBe(
            [
                ("/api/contact/address-types", "GET"),
                ("/api/contact/address-types/{id:guid}", "GET"),
                ("/api/contact/address-types", "POST"),
                ("/api/contact/address-types/{id:guid}", "PUT"),
                ("/api/contact/address-types/{id:guid}", "DELETE")
            ],
            ignoreOrder: true);
    }

    [Fact]
    public void MapAddressTypeEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapAddressTypeEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
