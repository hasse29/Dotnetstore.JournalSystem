using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies;

public sealed class OwnCompanyConfigurationTests
{
    private static IEntityType BuildEntityType()
    {
        using var context = new TestDbContext();
        return context.Model.FindEntityType(typeof(OwnCompany)).ShouldNotBeNull();
    }

    private const string ExpectedTableName = "OwnCompany";
    private const string ExpectedSchema = "Organization";

    [Fact]
    public void ConfigureMapsOwnCompanyToTableNamedOwnCompany()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.GetTableName().ShouldBe(ExpectedTableName);
    }

    [Fact]
    public void ConfigureDoesNotUseBaseTypeNameAsTableName()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.GetTableName().ShouldNotBe("Company");
    }

    [Fact]
    public void ConfigurePlacesTableInOrganizationSchema()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.GetSchema().ShouldBe(ExpectedSchema);
    }

    [Fact]
    public void ConfigureDoesNotUseDefaultOrOtherSchema()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.GetSchema().ShouldNotBeNull();
        entityType.GetSchema().ShouldNotBe("dbo");
        entityType.GetSchema().ShouldNotBe("organization");
        entityType.GetSchema().ShouldNotBe(ExpectedTableName);
    }

    [Fact]
    public void ConfigureProducesSchemaQualifiedStoreIdentifier()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var storeObject = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table).ShouldNotBeNull();
        storeObject.Schema.ShouldBe(ExpectedSchema);
        storeObject.Name.ShouldBe(ExpectedTableName);
    }

    [Fact]
    public void ConfigureUsesIdAsPrimaryKey()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var key = entityType.FindPrimaryKey().ShouldNotBeNull();
        key.Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(OwnCompany.Id));
    }

    [Fact]
    public void ConfigureDoesNotUseAnyOtherPropertyAsPrimaryKey()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var key = entityType.FindPrimaryKey().ShouldNotBeNull();
        key.Properties.Select(p => p.Name).ShouldNotContain(nameof(OwnCompany.Name));
        key.Properties.Count.ShouldBe(1);
    }

    [Fact]
    public void ConfigureAppliesBaseCompanyConfigurationRequiredName()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var name = entityType.FindProperty(nameof(OwnCompany.Name)).ShouldNotBeNull();
        name.IsNullable.ShouldBeFalse();
        name.GetMaxLength().ShouldBe(200);
    }

    [Fact]
    public void ConfigureLeavesOptionalPropertiesNullable()
    {
        // Arrange
        string[] optional =
        [
            nameof(OwnCompany.Description),
            nameof(OwnCompany.LogoUrl),
            nameof(OwnCompany.WebsiteUrl),
            nameof(OwnCompany.CorporateNumber),
            nameof(OwnCompany.TaxId)
        ];

        // Act
        var entityType = BuildEntityType();

        // Assert
        foreach (var propertyName in optional)
        {
            entityType.FindProperty(propertyName).ShouldNotBeNull().IsNullable.ShouldBeTrue(propertyName);
        }
    }

    [Fact]
    public void ConfigureDoesNotMapUnknownProperties()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.FindProperty("DoesNotExist").ShouldBeNull();
    }

    [Fact]
    public void ConfigureRegistersOnlyOwnCompanyInTheModel()
    {
        // Arrange & Act
        using var context = new TestDbContext();

        // Assert
        context.Model.GetEntityTypes().ShouldHaveSingleItem().ClrType.ShouldBe(typeof(OwnCompany));
    }

    private sealed class TestDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite("Data Source=:memory:");

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfiguration(new OwnCompanyConfiguration());
    }
}
