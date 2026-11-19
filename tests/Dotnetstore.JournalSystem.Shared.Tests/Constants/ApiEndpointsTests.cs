using Dotnetstore.JournalSystem.Shared.Constants;
using Shouldly;

namespace Dotnetstore.JournalSystem.Shared.Tests.Constants;

public sealed class ApiEndpointsTests
{
    [Fact]
    public void OwnCompanyCollectionRoutesUseTheOrganizationPath()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Organization.OwnCompanies.GetAll,
            ApiEndpoints.Organization.OwnCompanies.Create
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/organization/own-companies");
    }

    [Fact]
    public void OwnCompanyItemRoutesRequireAGuidId()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Organization.OwnCompanies.GetById,
            ApiEndpoints.Organization.OwnCompanies.Update,
            ApiEndpoints.Organization.OwnCompanies.Delete
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/organization/own-companies/{id:guid}");
    }

    [Fact]
    public void AllRoutesStartWithApiPrefixAndHaveNoTrailingSlash()
    {
        // Arrange
        var routes = new[]
        {
            ApiEndpoints.Organization.OwnCompanies.GetAll,
            ApiEndpoints.Organization.OwnCompanies.GetById,
            ApiEndpoints.Organization.OwnCompanies.Create,
            ApiEndpoints.Organization.OwnCompanies.Update,
            ApiEndpoints.Organization.OwnCompanies.Delete
        };

        // Act & Assert
        routes.ShouldAllBe(x => x.StartsWith("/api/", StringComparison.Ordinal) && !x.EndsWith('/'));
    }

    [Fact]
    public void ItemRoutesDoNotMatchCollectionRoutes()
    {
        // Arrange & Act & Assert
        ApiEndpoints.Organization.OwnCompanies.GetById.ShouldNotBe(ApiEndpoints.Organization.OwnCompanies.GetAll);
    }

    [Fact]
    public void RoleCollectionRoutesUseTheOrganizationPath()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Organization.Roles.GetAll,
            ApiEndpoints.Organization.Roles.Create
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/organization/roles");
    }

    [Fact]
    public void RoleItemRoutesRequireAGuidId()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Organization.Roles.GetById,
            ApiEndpoints.Organization.Roles.Update,
            ApiEndpoints.Organization.Roles.Delete
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/organization/roles/{id:guid}");
    }

    [Fact]
    public void RoleRoutesStartWithApiPrefixAndHaveNoTrailingSlash()
    {
        // Arrange
        var routes = new[]
        {
            ApiEndpoints.Organization.Roles.GetAll,
            ApiEndpoints.Organization.Roles.GetById,
            ApiEndpoints.Organization.Roles.Create,
            ApiEndpoints.Organization.Roles.Update,
            ApiEndpoints.Organization.Roles.Delete
        };

        // Act & Assert
        routes.ShouldAllBe(x => x.StartsWith("/api/", StringComparison.Ordinal) && !x.EndsWith('/'));
    }

    [Fact]
    public void RoleRoutesDoNotOverlapWithOwnCompanyRoutes()
    {
        // Arrange & Act & Assert
        ApiEndpoints.Organization.Roles.GetAll.ShouldNotBe(ApiEndpoints.Organization.OwnCompanies.GetAll);
        ApiEndpoints.Organization.Roles.GetById.ShouldNotBe(ApiEndpoints.Organization.Roles.GetAll);
    }
}
