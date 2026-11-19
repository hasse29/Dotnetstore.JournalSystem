using Dotnetstore.JournalSystem.Api.Contact.Extensions;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;

public sealed class PhoneNumberEndpointsTests
{
    [Fact]
    public void MapPhoneNumberEndpointsRegistersAllRoutes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = "Server=localhost,1;Database=journalsystem;User Id=u;Password=p;TrustServerCertificate=True";
        builder.RegisterContactServices();
        using var app = builder.Build();

        // Act
        app.MapPhoneNumberEndpoints();

        // Assert
        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(x => x.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(x => (Pattern: x.RoutePattern.RawText?.TrimEnd('/'), Method: x.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single()))
            .ToList();
        routes.ShouldBe(
            [
                ("/api/contact/phone-numbers", "GET"),
                ("/api/contact/phone-numbers/{id:guid}", "GET"),
                ("/api/contact/phone-numbers", "POST"),
                ("/api/contact/phone-numbers/{id:guid}", "PUT"),
                ("/api/contact/phone-numbers/{id:guid}", "DELETE")
            ],
            ignoreOrder: true);
    }

    [Fact]
    public void MapPhoneNumberEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapPhoneNumberEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }
}
