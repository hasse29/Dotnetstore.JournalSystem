using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;

public sealed class OwnCompanyTests
{
    private static readonly Guid CompanyId = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");
    private static readonly Uri LogoUrl = new("https://example.com/logo.png");
    private static readonly Uri WebsiteUrl = new("https://example.com");

    [Fact]
    public void ConstructorWithAllValuesSetsAllProperties()
    {
        // Arrange
        const string name = "Dotnetstore AB";
        const string description = "Journal system vendor";
        const string corporateNumber = "556677-8899";
        const string taxId = "SE556677889901";

        // Act
        var company = OwnCompanyTestData.Create(CompanyId, name, description, LogoUrl, WebsiteUrl, corporateNumber, taxId);

        // Assert
        company.Id.ShouldBe(CompanyId);
        company.Name.ShouldBe(name);
        company.Description.ShouldBe(description);
        company.LogoUrl.ShouldBe(LogoUrl);
        company.WebsiteUrl.ShouldBe(WebsiteUrl);
        company.CorporateNumber.ShouldBe(corporateNumber);
        company.TaxId.ShouldBe(taxId);
    }

    [Fact]
    public void ConstructorSetsAuditValues()
    {
        // Arrange
        var user = Guid.NewGuid();
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var updated = created.AddDays(1);
        var deleted = created.AddDays(2);

        // Act
        var company = new OwnCompany(CompanyId, "Dotnetstore AB", null, null, null, null, null, created, user, updated, user, deleted, user, true, true, true)
        {
            Name = "Dotnetstore AB"
        };

        // Assert
        company.CreatedDate.ShouldBe(created);
        company.CreatedBy.ShouldBe(user);
        company.LastUpdatedDate.ShouldBe(updated);
        company.LastUpdatedBy.ShouldBe(user);
        company.DeletedDate.ShouldBe(deleted);
        company.DeletedBy.ShouldBe(user);
        company.IsDeleted.ShouldBeTrue();
        company.IsSystem.ShouldBeTrue();
        company.IsGdpr.ShouldBeTrue();
    }

    [Fact]
    public void ConstructorIsACompany()
    {
        // Arrange & Act
        var company = OwnCompanyTestData.Create(CompanyId, "Dotnetstore AB", null, null, null, null, null);

        // Assert
        company.ShouldBeAssignableTo<Company>();
        company.ShouldBeAssignableTo<Auditable>();
    }

    [Fact]
    public void ConstructorWithoutOptionalValuesLeavesThemNull()
    {
        // Arrange & Act
        var company = OwnCompanyTestData.Create(CompanyId, "Dotnetstore AB", null, null, null, null, null);

        // Assert
        company.Description.ShouldBeNull();
        company.LogoUrl.ShouldBeNull();
        company.WebsiteUrl.ShouldBeNull();
        company.CorporateNumber.ShouldBeNull();
        company.TaxId.ShouldBeNull();
    }

    [Fact]
    public void ConstructorWithoutOptionalValuesStillSetsRequiredValues()
    {
        // Arrange
        const string name = "Dotnetstore AB";

        // Act
        var company = OwnCompanyTestData.Create(CompanyId, name, null, null, null, null, null);

        // Assert
        company.Id.ShouldBe(CompanyId);
        company.Name.ShouldBe(name);
    }

    [Fact]
    public void DifferentConstructorValuesProduceIndependentCompanies()
    {
        // Arrange
        var otherId = Guid.NewGuid();

        // Act
        var first = OwnCompanyTestData.Create(CompanyId, "First AB", "First", LogoUrl, WebsiteUrl, "111", "T1");
        var second = OwnCompanyTestData.Create(otherId, "Second AB", null, null, null, null, null);

        // Assert
        second.Id.ShouldNotBe(first.Id);
        second.Name.ShouldNotBe(first.Name);
        first.Description.ShouldBe("First");
        second.Description.ShouldBeNull();
        first.CorporateNumber.ShouldBe("111");
        second.CorporateNumber.ShouldBeNull();
    }
}
