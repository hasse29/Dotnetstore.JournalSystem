using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;

public sealed class RoleConfigurationTests
{
    private const string ExpectedTableName = "Role";
    private const string ExpectedSchema = "Organization";

    private static IEntityType BuildEntityType()
    {
        using var context = new TestDbContext();
        return context.Model.FindEntityType(typeof(Role)).ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureMapsRoleToTableNamedRole()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.GetTableName().ShouldBe(ExpectedTableName);
    }

    [Fact]
    public void ConfigurePlacesTableInOrganizationSchema()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.GetSchema().ShouldBe(ExpectedSchema);
        entityType.GetSchema().ShouldNotBe("dbo");
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
        key.Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(Role.Id));
    }

    [Fact]
    public void ConfigureMakesNameRequiredWithMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var name = entityType.FindProperty(nameof(Role.Name)).ShouldNotBeNull();
        name.IsNullable.ShouldBeFalse();
        name.GetMaxLength().ShouldBe(DataSchemeConstants.RoleNameMaxLength);
    }

    [Fact]
    public void ConfigureMakesDescriptionRequiredWithMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var description = entityType.FindProperty(nameof(Role.Description)).ShouldNotBeNull();
        description.IsNullable.ShouldBeFalse();
        description.GetMaxLength().ShouldBe(DataSchemeConstants.RoleDescriptionMaxLength);
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
    public void ConfigureRegistersOnlyRoleInTheModel()
    {
        // Arrange & Act
        using var context = new TestDbContext();

        // Assert
        context.Model.GetEntityTypes().ShouldHaveSingleItem().ClrType.ShouldBe(typeof(Role));
    }

    [Fact]
    public void ConfigureWithNullBuilderThrows()
    {
        // Arrange
        var configuration = new RoleConfiguration();

        // Act
        var action = () => configuration.Configure(null!);

        // Assert
        action.ShouldThrow<ArgumentNullException>();
    }

    private sealed class TestDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite("Data Source=:memory:");

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
    }
}
