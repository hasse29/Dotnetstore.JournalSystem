using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;

public sealed class OwnCompanyMappersTests
{
    [Fact]
    public void ToOwnCompanyResponseMapsAllValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var logo = new Uri("https://example.com/logo.png");
        var website = new Uri("https://example.com");
        var company = OwnCompanyTestData.Create(id, "Dotnetstore AB", "Description", logo, website, "556677-8899", "SE556677889901");

        // Act
        var response = company.ToOwnCompanyResponse();

        // Assert
        response.Id.ShouldBe(id);
        response.Name.ShouldBe("Dotnetstore AB");
        response.Description.ShouldBe("Description");
        response.LogoUrl.ShouldBe(logo);
        response.WebsiteUrl.ShouldBe(website);
        response.CorporateNumber.ShouldBe("556677-8899");
        response.TaxId.ShouldBe("SE556677889901");
    }

    [Fact]
    public void ToOwnCompanyResponseKeepsOptionalValuesNull()
    {
        // Arrange
        var company = OwnCompanyTestData.Create(Guid.NewGuid(), "Dotnetstore AB");

        // Act
        var response = company.ToOwnCompanyResponse();

        // Assert
        response.Description.ShouldBeNull();
        response.LogoUrl.ShouldBeNull();
        response.WebsiteUrl.ShouldBeNull();
        response.CorporateNumber.ShouldBeNull();
        response.TaxId.ShouldBeNull();
    }
}
