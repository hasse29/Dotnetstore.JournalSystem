using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;

public sealed class OwnCompanyBuilderTests
{
    private static readonly Uri Logo = new("https://example.com/logo.png");
    private static readonly Uri Website = new("https://example.com");

    private static EditOwnCompanyRequest CreateRequest(string name = "Dotnetstore AB")
    {
        return new EditOwnCompanyRequest(name, "Description", Logo, Website, "556677-8899", "SE556677889901");
    }

    [Fact]
    public void CreateWithCreateValuesBuildsCompanyFromRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var company = OwnCompanyBuilder.Create().WithCreateValues(CreateRequest(), userId).Build();

        // Assert
        company.Id.ShouldNotBe(Guid.Empty);
        company.Name.ShouldBe("Dotnetstore AB");
        company.Description.ShouldBe("Description");
        company.LogoUrl.ShouldBe(Logo);
        company.WebsiteUrl.ShouldBe(Website);
        company.CorporateNumber.ShouldBe("556677-8899");
        company.TaxId.ShouldBe("SE556677889901");
        company.CreatedBy.ShouldBe(userId);
        company.CreatedDate.ShouldBeInRange(before, DateTime.UtcNow);
        company.LastUpdatedDate.ShouldBeNull();
        company.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void CreateWithCreateValuesGeneratesNewIdEachTime()
    {
        // Arrange & Act
        var first = OwnCompanyBuilder.Create().WithCreateValues(CreateRequest(), null).Build();
        var second = OwnCompanyBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void CreateWithCreateValuesAllowsMissingUser()
    {
        // Arrange & Act
        var company = OwnCompanyBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        company.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void CreateWithCreateValuesRejectsEmptyUserId()
    {
        // Arrange & Act
        var act = () => OwnCompanyBuilder.Create().WithCreateValues(CreateRequest(), Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithCreateValuesRejectsBlankName(string name)
    {
        // Arrange & Act
        var act = () => OwnCompanyBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsTooLongName()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.OwnCompanyNameMaxLength + 1);

        // Act
        var act = () => OwnCompanyBuilder.Create().WithCreateValues(CreateRequest(name), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesAcceptsNameAtMaxLength()
    {
        // Arrange
        var name = new string('a', DataSchemeConstants.OwnCompanyNameMaxLength);

        // Act
        var company = OwnCompanyBuilder.Create().WithCreateValues(CreateRequest(name), null).Build();

        // Assert
        company.Name.ShouldBe(name);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsIdentityAndCreationData()
    {
        // Arrange
        var creator = Guid.NewGuid();
        var existing = OwnCompanyBuilder.Create().WithCreateValues(CreateRequest(), creator).Build();
        var editor = Guid.NewGuid();
        var request = new EditOwnCompanyRequest("Renamed AB", null, null, null, null, null);
        var before = DateTime.UtcNow;

        // Act
        var updated = OwnCompanyBuilder.Update().WithUpdateValues(existing, request, editor).Build();

        // Assert
        updated.Id.ShouldBe(existing.Id);
        updated.CreatedDate.ShouldBe(existing.CreatedDate);
        updated.CreatedBy.ShouldBe(creator);
        updated.Name.ShouldBe("Renamed AB");
        updated.Description.ShouldBeNull();
        updated.TaxId.ShouldBeNull();
        updated.LastUpdatedBy.ShouldBe(editor);
        updated.LastUpdatedDate.ShouldNotBeNull().ShouldBeInRange(before, DateTime.UtcNow);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsFlagsFromExistingCompany()
    {
        // Arrange
        var existing = OwnCompanyBuilder.Delete()
            .SetId(Guid.NewGuid())
            .SetName("Dotnetstore AB")
            .SetCreatedDate(OwnCompanyTestData.CreatedDate)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Act
        var updated = OwnCompanyBuilder.Update().WithUpdateValues(existing, CreateRequest("Other AB"), null).Build();

        // Assert
        updated.IsSystem.ShouldBeTrue();
        updated.IsGdpr.ShouldBeTrue();
        updated.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void UpdateWithUpdateValuesRejectsInvalidName()
    {
        // Arrange
        var existing = OwnCompanyBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Act
        var act = () => OwnCompanyBuilder.Update().WithUpdateValues(existing, CreateRequest(""), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetIdRejectsEmptyGuid()
    {
        // Arrange
        var builder = OwnCompanyBuilder.Delete();

        // Act
        var act = () => builder.SetId(Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetCreatedDateRejectsDefaultDate()
    {
        // Arrange
        var builder = OwnCompanyBuilder.Delete();

        // Act
        var act = () => builder.SetCreatedDate(default);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetDescriptionRejectsTooLongValueButAcceptsMaxLength()
    {
        // Arrange
        var builder = OwnCompanyBuilder.Delete();

        // Act
        var tooLong = () => builder.SetDescription(new string('a', DataSchemeConstants.OwnCompanyDescriptionMaxLength + 1));
        builder.SetDescription(new string('a', DataSchemeConstants.OwnCompanyDescriptionMaxLength));

        // Assert
        Should.Throw<ArgumentException>(tooLong);
    }

    [Fact]
    public void SetCorporateNumberRejectsTooLongValueButAcceptsMaxLength()
    {
        // Arrange
        var builder = OwnCompanyBuilder.Delete();

        // Act
        var tooLong = () => builder.SetCorporateNumber(new string('1', DataSchemeConstants.OwnCompanyCorporateNumberMaxLength + 1));
        builder.SetCorporateNumber(new string('1', DataSchemeConstants.OwnCompanyCorporateNumberMaxLength));

        // Assert
        Should.Throw<ArgumentException>(tooLong);
    }

    [Fact]
    public void SetTaxIdRejectsTooLongValueButAcceptsMaxLength()
    {
        // Arrange
        var builder = OwnCompanyBuilder.Delete();

        // Act
        var tooLong = () => builder.SetTaxId(new string('1', DataSchemeConstants.OwnCompanyTaxIdMaxLength + 1));
        builder.SetTaxId(new string('1', DataSchemeConstants.OwnCompanyTaxIdMaxLength));

        // Assert
        Should.Throw<ArgumentException>(tooLong);
    }

    [Fact]
    public void OptionalSettersAcceptNullAndBlank()
    {
        // Arrange
        var builder = OwnCompanyBuilder.Delete();

        // Act
        var act = () => builder
            .SetDescription(null)
            .SetCorporateNumber("  ")
            .SetTaxId(null)
            .SetLogoUrl(null)
            .SetWebsiteUrl(null);

        // Assert
        Should.NotThrow(act);
    }

    [Fact]
    public void AuditSettersRejectEmptyUserGuids()
    {
        // Arrange
        var builder = OwnCompanyBuilder.Delete();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.SetCreatedBy(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetLastUpdatedBy(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetDeletedBy(Guid.Empty));
    }

    [Fact]
    public void BuildCarriesAllSetValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = Guid.NewGuid();
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var deleted = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var company = OwnCompanyBuilder.Delete()
            .SetId(id)
            .SetName("Dotnetstore AB")
            .SetCreatedDate(created)
            .SetCreatedBy(user)
            .SetDeletedDate(deleted)
            .SetDeletedBy(user)
            .SetIsDeleted(true)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Assert
        company.Id.ShouldBe(id);
        company.Name.ShouldBe("Dotnetstore AB");
        company.CreatedDate.ShouldBe(created);
        company.DeletedDate.ShouldBe(deleted);
        company.DeletedBy.ShouldBe(user);
        company.IsDeleted.ShouldBeTrue();
        company.IsSystem.ShouldBeTrue();
        company.IsGdpr.ShouldBeTrue();
    }
}
