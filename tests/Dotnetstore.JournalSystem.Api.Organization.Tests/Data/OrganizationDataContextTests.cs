using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Data;

public sealed class OrganizationDataContextTests
{
    private static OrganizationDataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        return new OrganizationDataContext(options);
    }

    [Fact]
    public void ModelContainsOwnCompanyEntity()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(OwnCompany));

        // Assert
        entityType.ShouldNotBeNull();
    }

    [Fact]
    public void ModelContainsOnlyOwnCompanyEntity()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityTypes = context.Model.GetEntityTypes().ToList();

        // Assert
        entityTypes.ShouldHaveSingleItem().ClrType.ShouldBe(typeof(OwnCompany));
    }

    [Fact]
    public void ModelDoesNotContainAbstractBaseTypes()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var company = context.Model.FindEntityType(typeof(Company));
        var auditable = context.Model.FindEntityType(typeof(Auditable));

        // Assert
        company.ShouldBeNull();
        auditable.ShouldBeNull();
    }

    [Fact]
    public void OwnCompaniesDbSetIsExposed()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var dbSet = context.OwnCompanies;

        // Assert
        dbSet.ShouldNotBeNull();
        dbSet.EntityType.ClrType.ShouldBe(typeof(OwnCompany));
    }

    [Fact]
    public void OwnCompaniesDbSetIsTheSameAsSetOfOwnCompany()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var viaSet = context.Set<OwnCompany>();

        // Assert
        viaSet.EntityType.ShouldBeSameAs(context.OwnCompanies.EntityType);
    }

    [Fact]
    public void ModelAppliesOwnCompanyConfigurationFromAssembly()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(OwnCompany)).ShouldNotBeNull();

        // Assert
        entityType.GetTableName().ShouldBe("OwnCompanies");
        entityType.GetSchema().ShouldBe("Organization");
        entityType.FindPrimaryKey().ShouldNotBeNull().Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(OwnCompany.Id));
    }

    [Fact]
    public void ModelDoesNotMapOwnCompanyToDefaultTableOrSchema()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(OwnCompany)).ShouldNotBeNull();

        // Assert
        entityType.GetTableName().ShouldNotBe(nameof(OwnCompany));
        entityType.GetSchema().ShouldNotBeNull();
    }

    [Fact]
    public void SetOfEntityNotInModelThrows()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var action = () => context.Set<Company>().EntityType;

        // Assert
        action.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void ContextUsesProvidedOptions()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var providerName = context.Database.ProviderName;

        // Assert
        providerName.ShouldBe("Microsoft.EntityFrameworkCore.Sqlite");
        providerName.ShouldNotBe("Microsoft.EntityFrameworkCore.SqlServer");
    }
}
