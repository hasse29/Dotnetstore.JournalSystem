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

    [Fact]
    public void WorkGroupCollectionRoutesUseTheOrganizationPath()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Organization.WorkGroups.GetAll,
            ApiEndpoints.Organization.WorkGroups.Create
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/organization/work-groups");
    }

    [Fact]
    public void WorkGroupItemRoutesRequireAGuidId()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Organization.WorkGroups.GetById,
            ApiEndpoints.Organization.WorkGroups.Update,
            ApiEndpoints.Organization.WorkGroups.Delete
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/organization/work-groups/{id:guid}");
    }

    [Fact]
    public void WorkGroupRoutesStartWithApiPrefixAndHaveNoTrailingSlash()
    {
        // Arrange
        var routes = new[]
        {
            ApiEndpoints.Organization.WorkGroups.GetAll,
            ApiEndpoints.Organization.WorkGroups.GetById,
            ApiEndpoints.Organization.WorkGroups.Create,
            ApiEndpoints.Organization.WorkGroups.Update,
            ApiEndpoints.Organization.WorkGroups.Delete
        };

        // Act & Assert
        routes.ShouldAllBe(x => x.StartsWith("/api/", StringComparison.Ordinal) && !x.EndsWith('/'));
    }

    [Fact]
    public void WorkGroupRoutesDoNotOverlapWithOtherOrganizationRoutes()
    {
        // Arrange & Act & Assert
        ApiEndpoints.Organization.WorkGroups.GetAll.ShouldNotBe(ApiEndpoints.Organization.Roles.GetAll);
        ApiEndpoints.Organization.WorkGroups.GetAll.ShouldNotBe(ApiEndpoints.Organization.OwnCompanies.GetAll);
        ApiEndpoints.Organization.WorkGroups.GetById.ShouldNotBe(ApiEndpoints.Organization.WorkGroups.GetAll);
    }

    [Fact]
    public void AddressTypeCollectionRoutesUseTheContactPath()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Contact.AddressTypes.GetAll,
            ApiEndpoints.Contact.AddressTypes.Create
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/contact/address-types");
    }

    [Fact]
    public void AddressTypeItemRoutesRequireAGuidId()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Contact.AddressTypes.GetById,
            ApiEndpoints.Contact.AddressTypes.Update,
            ApiEndpoints.Contact.AddressTypes.Delete
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/contact/address-types/{id:guid}");
    }

    [Fact]
    public void AddressTypeRoutesStartWithApiPrefixAndHaveNoTrailingSlash()
    {
        // Arrange
        var routes = new[]
        {
            ApiEndpoints.Contact.AddressTypes.GetAll,
            ApiEndpoints.Contact.AddressTypes.GetById,
            ApiEndpoints.Contact.AddressTypes.Create,
            ApiEndpoints.Contact.AddressTypes.Update,
            ApiEndpoints.Contact.AddressTypes.Delete
        };

        // Act & Assert
        routes.ShouldAllBe(x => x.StartsWith("/api/", StringComparison.Ordinal) && !x.EndsWith('/'));
    }

    [Fact]
    public void AddressTypeRoutesDoNotOverlapWithOrganizationRoutes()
    {
        // Arrange & Act & Assert
        ApiEndpoints.Contact.AddressTypes.GetAll.ShouldNotStartWith(ApiEndpoints.Organization.Roles.GetAll);
        ApiEndpoints.Contact.AddressTypes.GetAll.ShouldNotBe(ApiEndpoints.Organization.OwnCompanies.GetAll);
        ApiEndpoints.Contact.AddressTypes.GetById.ShouldNotBe(ApiEndpoints.Contact.AddressTypes.GetAll);
    }

    [Fact]
    public void AddressCollectionRoutesUseTheContactPath()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Contact.Addresses.GetAll,
            ApiEndpoints.Contact.Addresses.Create
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/contact/addresses");
    }

    [Fact]
    public void AddressItemRoutesRequireAGuidId()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Contact.Addresses.GetById,
            ApiEndpoints.Contact.Addresses.Update,
            ApiEndpoints.Contact.Addresses.Delete
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/contact/addresses/{id:guid}");
    }

    [Fact]
    public void AddressRoutesStartWithApiPrefixAndHaveNoTrailingSlash()
    {
        // Arrange
        var routes = new[]
        {
            ApiEndpoints.Contact.Addresses.GetAll,
            ApiEndpoints.Contact.Addresses.GetById,
            ApiEndpoints.Contact.Addresses.Create,
            ApiEndpoints.Contact.Addresses.Update,
            ApiEndpoints.Contact.Addresses.Delete
        };

        // Act & Assert
        routes.ShouldAllBe(x => x.StartsWith("/api/", StringComparison.Ordinal) && !x.EndsWith('/'));
    }

    [Fact]
    public void AddressRoutesDoNotOverlapWithAddressTypeRoutes()
    {
        // Arrange & Act & Assert
        ApiEndpoints.Contact.Addresses.GetAll.ShouldNotBe(ApiEndpoints.Contact.AddressTypes.GetAll);
        ApiEndpoints.Contact.Addresses.GetById.ShouldNotBe(ApiEndpoints.Contact.Addresses.GetAll);
    }

    [Fact]
    public void EmailAddressTypeCollectionRoutesUseTheContactPath()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Contact.EmailAddressTypes.GetAll,
            ApiEndpoints.Contact.EmailAddressTypes.Create
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/contact/email-address-types");
    }

    [Fact]
    public void EmailAddressTypeItemRoutesRequireAGuidId()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Contact.EmailAddressTypes.GetById,
            ApiEndpoints.Contact.EmailAddressTypes.Update,
            ApiEndpoints.Contact.EmailAddressTypes.Delete
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/contact/email-address-types/{id:guid}");
    }

    [Fact]
    public void EmailAddressTypeRoutesStartWithApiPrefixAndHaveNoTrailingSlash()
    {
        // Arrange
        var routes = new[]
        {
            ApiEndpoints.Contact.EmailAddressTypes.GetAll,
            ApiEndpoints.Contact.EmailAddressTypes.GetById,
            ApiEndpoints.Contact.EmailAddressTypes.Create,
            ApiEndpoints.Contact.EmailAddressTypes.Update,
            ApiEndpoints.Contact.EmailAddressTypes.Delete
        };

        // Act & Assert
        routes.ShouldAllBe(x => x.StartsWith("/api/", StringComparison.Ordinal) && !x.EndsWith('/'));
    }

    [Fact]
    public void EmailAddressTypeRoutesDoNotOverlapWithAddressTypeRoutes()
    {
        // Arrange & Act & Assert
        ApiEndpoints.Contact.EmailAddressTypes.GetAll.ShouldNotBe(ApiEndpoints.Contact.AddressTypes.GetAll);
        ApiEndpoints.Contact.EmailAddressTypes.GetById.ShouldNotBe(ApiEndpoints.Contact.EmailAddressTypes.GetAll);
    }

    [Fact]
    public void EmailAddressCollectionRoutesUseTheContactPath()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Contact.EmailAddresses.GetAll,
            ApiEndpoints.Contact.EmailAddresses.Create
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/contact/email-addresses");
    }

    [Fact]
    public void EmailAddressItemRoutesRequireAGuidId()
    {
        // Arrange & Act
        var routes = new[]
        {
            ApiEndpoints.Contact.EmailAddresses.GetById,
            ApiEndpoints.Contact.EmailAddresses.Update,
            ApiEndpoints.Contact.EmailAddresses.Delete
        };

        // Assert
        routes.ShouldAllBe(x => x == "/api/contact/email-addresses/{id:guid}");
    }

    [Fact]
    public void EmailAddressRoutesStartWithApiPrefixAndHaveNoTrailingSlash()
    {
        // Arrange
        var routes = new[]
        {
            ApiEndpoints.Contact.EmailAddresses.GetAll,
            ApiEndpoints.Contact.EmailAddresses.GetById,
            ApiEndpoints.Contact.EmailAddresses.Create,
            ApiEndpoints.Contact.EmailAddresses.Update,
            ApiEndpoints.Contact.EmailAddresses.Delete
        };

        // Act & Assert
        routes.ShouldAllBe(x => x.StartsWith("/api/", StringComparison.Ordinal) && !x.EndsWith('/'));
    }

    [Fact]
    public void EmailAddressRoutesDoNotOverlapWithEmailAddressTypeRoutes()
    {
        // Arrange & Act & Assert
        ApiEndpoints.Contact.EmailAddresses.GetAll.ShouldNotBe(ApiEndpoints.Contact.EmailAddressTypes.GetAll);
        ApiEndpoints.Contact.EmailAddresses.GetById.ShouldNotBe(ApiEndpoints.Contact.EmailAddresses.GetAll);
    }
}
