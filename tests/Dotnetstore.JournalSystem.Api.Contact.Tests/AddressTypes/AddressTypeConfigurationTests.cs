using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.AddressTypes;

public sealed class AddressTypeConfigurationTests
{
    private const string ExpectedTableName = "AddressType";
    private const string ExpectedSchema = "Contact";

    private static IEntityType BuildEntityType()
    {
        using var context = new TestDbContext();
        return context.Model.FindEntityType(typeof(AddressType)).ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureMapsAddressTypeToTableNamedAddressType()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.GetTableName().ShouldBe(ExpectedTableName);
    }

    [Fact]
    public void ConfigurePlacesTableInContactSchema()
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
        key.Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(AddressType.Id));
    }

    [Fact]
    public void ConfigureMakesNameRequiredWithMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var name = entityType.FindProperty(nameof(AddressType.Name)).ShouldNotBeNull();
        name.IsNullable.ShouldBeFalse();
        name.GetMaxLength().ShouldBe(DataSchemeConstants.AddressTypeNameMaxLength);
    }

    [Fact]
    public void ConfigureMakesDescriptionRequiredWithMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var description = entityType.FindProperty(nameof(AddressType.Description)).ShouldNotBeNull();
        description.IsNullable.ShouldBeFalse();
        description.GetMaxLength().ShouldBe(DataSchemeConstants.AddressTypeDescriptionMaxLength);
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
    public void ConfigureRegistersOnlyAddressTypeInTheModel()
    {
        // Arrange & Act
        using var context = new TestDbContext();

        // Assert
        context.Model.GetEntityTypes().ShouldHaveSingleItem().ClrType.ShouldBe(typeof(AddressType));
    }

    [Fact]
    public void ConfigureWithNullBuilderThrows()
    {
        // Arrange
        var configuration = new AddressTypeConfiguration();

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
            modelBuilder.ApplyConfiguration(new AddressTypeConfiguration());
    }
}
