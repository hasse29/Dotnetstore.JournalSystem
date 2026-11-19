using Dotnetstore.JournalSystem.Api.Contact.Extensions;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddressTypes;

public sealed class EmailAddressTypeEndpointsTests
{
    [Fact]
    public void MapEmailAddressTypeEndpointsRegistersAllRoutes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = "Server=localhost,1;Database=journalsystem;User Id=u;Password=p;TrustServerCertificate=True";
        builder.RegisterContactServices();
        using var app = builder.Build();

        // Act
        app.MapEmailAddressTypeEndpoints();

        // Assert
        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(x => x.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(x => (Pattern: x.RoutePattern.RawText?.TrimEnd('/'), Method: x.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single()))
            .ToList();
        routes.ShouldBe(
            [
                ("/api/contact/email-address-types", "GET"),
                ("/api/contact/email-address-types/{id:guid}", "GET"),
                ("/api/contact/email-address-types", "POST"),
                ("/api/contact/email-address-types/{id:guid}", "PUT"),
                ("/api/contact/email-address-types/{id:guid}", "DELETE")
            ],
            ignoreOrder: true);
    }

    [Fact]
    public void MapEmailAddressTypeEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapEmailAddressTypeEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
