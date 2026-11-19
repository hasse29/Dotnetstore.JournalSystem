using Dotnetstore.JournalSystem.Api.Contact.Extensions;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;

public sealed class EmailAddressEndpointsTests
{
    [Fact]
    public void MapEmailAddressEndpointsRegistersAllRoutes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = "Server=localhost,1;Database=journalsystem;User Id=u;Password=p;TrustServerCertificate=True";
        builder.RegisterContactServices();
        using var app = builder.Build();

        // Act
        app.MapEmailAddressEndpoints();

        // Assert
        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(x => x.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(x => (Pattern: x.RoutePattern.RawText?.TrimEnd('/'), Method: x.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single()))
            .ToList();
        routes.ShouldBe(
            [
                ("/api/contact/email-addresses", "GET"),
                ("/api/contact/email-addresses/{id:guid}", "GET"),
                ("/api/contact/email-addresses", "POST"),
                ("/api/contact/email-addresses/{id:guid}", "PUT"),
                ("/api/contact/email-addresses/{id:guid}", "DELETE")
            ],
            ignoreOrder: true);
    }

    [Fact]
    public void MapEmailAddressEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapEmailAddressEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
