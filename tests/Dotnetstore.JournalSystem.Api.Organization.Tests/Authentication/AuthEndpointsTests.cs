using System.Security.Claims;
using Dotnetstore.JournalSystem.Api.Organization.Authentication;
using Dotnetstore.JournalSystem.Api.Organization.Extensions;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Authentication;

public sealed class AuthEndpointsTests
{
    [Fact]
    public void MapAuthEndpointsRegistersLoginRoute()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["ConnectionStrings:" + HostApplicationBuilderExtensions.DatabaseConnectionName] = "Server=localhost,1;Database=journalsystem;User Id=u;Password=p;TrustServerCertificate=True";
        builder.RegisterOrganizationServices();
        using var app = builder.Build();

        // Act
        app.MapAuthEndpoints();

        // Assert
        var routes = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(x => x.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(x => (Pattern: x.RoutePattern.RawText, Method: x.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single()))
            .ToList();
        routes.ShouldBe([(ApiEndpoints.Auth.Login, "POST")]);
    }

    [Fact]
    public void MapAuthEndpointsWithNullBuilderThrows()
    {
        // Arrange
        IEndpointRouteBuilder app = null!;

        // Act
        var action = () => app.MapAuthEndpoints();

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void ValidateAcceptsUsernameAndPassword()
    {
        // Arrange & Act
        var errors = AuthEndpoints.Validate(new LoginRequest("anna@example.com", "secret"));

        // Assert
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void ValidateRejectsNullRequest()
    {
        // Arrange & Act
        var errors = AuthEndpoints.Validate(null);

        // Assert
        errors.ShouldContainKey("request");
    }

    [Theory]
    [InlineData("", "secret", "Username")]
    [InlineData("   ", "secret", "Username")]
    [InlineData("anna@example.com", "", "Password")]
    public void ValidateRejectsBlankFields(string username, string password, string expectedKey)
    {
        // Arrange & Act
        var errors = AuthEndpoints.Validate(new LoginRequest(username, password));

        // Assert
        errors.Keys.ShouldBe([expectedKey]);
    }

    [Fact]
    public void ValidateRejectsOversizedPasswordToAvoidExpensiveHashing()
    {
        // Arrange
        var request = new LoginRequest("anna@example.com", new string('a', DataSchemeConstants.UserPasswordMaxLength + 1));

        // Act
        var errors = AuthEndpoints.Validate(request);

        // Assert
        errors.ShouldContainKey("Password");
    }

    [Fact]
    public void GetUserIdReadsTheUserIdClaim()
    {
        // Arrange
        var id = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(AuthClaimTypes.UserId, id.ToString())], "test"));

        // Act
        var result = principal.GetUserId();

        // Assert
        result.ShouldBe(id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void GetUserIdReturnsNullForMissingOrInvalidClaim(string? value)
    {
        // Arrange
        var claims = value is null ? [] : new[] { new Claim(AuthClaimTypes.UserId, value) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        // Act
        var result = principal.GetUserId();

        // Assert
        result.ShouldBeNull();
    }
}
