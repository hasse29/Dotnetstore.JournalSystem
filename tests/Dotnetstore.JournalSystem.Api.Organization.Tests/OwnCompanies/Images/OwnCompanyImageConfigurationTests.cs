using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies.Images;

public sealed class OwnCompanyImageConfigurationTests
{
    private static IEntityType GetEntityType()
    {
        var options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlServer("Server=localhost,1;Database=unused;User Id=u;Password=p")
            .Options;
        using var context = new OrganizationDataContext(options);

        return context.Model.FindEntityType(typeof(OwnCompanyImage)).ShouldNotBeNull();
    }

    [Fact]
    public void ImagesAreStoredInTheOrganizationSchemaWithLimitedTextColumns()
    {
        // Arrange

        // Act
        var entity = GetEntityType();

        // Assert
        entity.GetSchema().ShouldBe(DataSchemeConstants.SchemeName);
        entity.FindProperty(nameof(OwnCompanyImage.FileName))!.GetMaxLength().ShouldBe(OwnCompanyImageConstants.FileNameMaxLength);
        entity.FindProperty(nameof(OwnCompanyImage.Content))!.IsNullable.ShouldBeFalse();
    }

    [Fact]
    public void ImagesAreDeletedWithTheirCompanyAndOnlyOneCanBeTheLogo()
    {
        // Arrange

        // Act
        var entity = GetEntityType();

        // Assert
        var foreignKey = entity.GetForeignKeys().ShouldHaveSingleItem();
        foreignKey.PrincipalEntityType.ClrType.ShouldBe(typeof(OwnCompany));
        foreignKey.DeleteBehavior.ShouldBe(DeleteBehavior.Cascade);
        var unique = entity.GetIndexes().Where(x => x.IsUnique).ShouldHaveSingleItem();
        unique.Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(OwnCompanyImage.OwnCompanyId));
    }
}
