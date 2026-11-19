using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumberTypes;

public sealed class PhoneNumberTypeConfigurationTests
{
    private const string ExpectedTableName = "PhoneNumberType";
    private const string ExpectedSchema = "Contact";

    private static IEntityType BuildEntityType()
    {
        using var context = new TestDbContext();
        return context.Model.FindEntityType(typeof(PhoneNumberType)).ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureMapsPhoneNumberTypeToTableNamedPhoneNumberType()
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
        key.Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(PhoneNumberType.Id));
    }

    [Fact]
    public void ConfigureMakesNameRequiredWithMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var name = entityType.FindProperty(nameof(PhoneNumberType.Name)).ShouldNotBeNull();
        name.IsNullable.ShouldBeFalse();
        name.GetMaxLength().ShouldBe(DataSchemeConstants.PhoneNumberTypeNameMaxLength);
    }

    [Fact]
    public void ConfigureMakesDescriptionRequiredWithMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var description = entityType.FindProperty(nameof(PhoneNumberType.Description)).ShouldNotBeNull();
        description.IsNullable.ShouldBeFalse();
        description.GetMaxLength().ShouldBe(DataSchemeConstants.PhoneNumberTypeDescriptionMaxLength);
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
    public void ConfigureRegistersOnlyPhoneNumberTypeInTheModel()
    {
        // Arrange & Act
        using var context = new TestDbContext();

        // Assert
        context.Model.GetEntityTypes().ShouldHaveSingleItem().ClrType.ShouldBe(typeof(PhoneNumberType));
    }

    [Fact]
    public void ConfigureWithNullBuilderThrows()
    {
        // Arrange
        var configuration = new PhoneNumberTypeConfiguration();

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
            modelBuilder.ApplyConfiguration(new PhoneNumberTypeConfiguration());
    }
}
