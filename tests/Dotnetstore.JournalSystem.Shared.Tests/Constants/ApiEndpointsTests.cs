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
}
