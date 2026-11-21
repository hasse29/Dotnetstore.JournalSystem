using Dotnetstore.JournalSystem.Api.Organization.AccessRequests;
using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Api.Organization.WorkGroups;
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
    public void ModelContainsOnlyOrganizationEntities()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityTypes = context.Model.GetEntityTypes().ToList();

        // Assert
        entityTypes.Select(x => x.ClrType).ShouldBe([typeof(AccessRequest), typeof(OwnCompany), typeof(Role), typeof(User), typeof(WorkGroup)], ignoreOrder: true);
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
    public void RolesDbSetIsExposed()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var dbSet = context.Roles;

        // Assert
        dbSet.ShouldNotBeNull();
        dbSet.EntityType.ClrType.ShouldBe(typeof(Role));
    }

    [Fact]
    public void ModelAppliesRoleConfigurationFromAssembly()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Role)).ShouldNotBeNull();

        // Assert
        entityType.GetTableName().ShouldBe("Role");
        entityType.GetSchema().ShouldBe("Organization");
    }

    [Fact]
    public void WorkGroupsDbSetIsExposed()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var dbSet = context.WorkGroups;

        // Assert
        dbSet.ShouldNotBeNull();
        dbSet.EntityType.ClrType.ShouldBe(typeof(WorkGroup));
    }

    [Fact]
    public void ModelAppliesWorkGroupConfigurationFromAssembly()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(WorkGroup)).ShouldNotBeNull();

        // Assert
        entityType.GetTableName().ShouldBe("WorkGroup");
        entityType.GetSchema().ShouldBe("Organization");
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
        entityType.GetTableName().ShouldBe("OwnCompany");
        entityType.GetSchema().ShouldBe("Organization");
        entityType.FindPrimaryKey().ShouldNotBeNull().Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(OwnCompany.Id));
    }

    [Fact]
    public void ModelDoesNotMapOwnCompanyToBaseTableOrDefaultSchema()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(OwnCompany)).ShouldNotBeNull();

        // Assert
        entityType.GetTableName().ShouldNotBe("Company");
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
