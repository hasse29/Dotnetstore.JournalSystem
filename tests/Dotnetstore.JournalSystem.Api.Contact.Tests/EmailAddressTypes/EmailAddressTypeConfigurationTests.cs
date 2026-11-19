using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddressTypes;

public sealed class EmailAddressTypeConfigurationTests
{
    private const string ExpectedTableName = "EmailAddressType";
    private const string ExpectedSchema = "Contact";

    private static IEntityType BuildEntityType()
    {
        using var context = new TestDbContext();
        return context.Model.FindEntityType(typeof(EmailAddressType)).ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureMapsEmailAddressTypeToTableNamedEmailAddressType()
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
        key.Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(EmailAddressType.Id));
    }

    [Fact]
    public void ConfigureMakesNameRequiredWithMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var name = entityType.FindProperty(nameof(EmailAddressType.Name)).ShouldNotBeNull();
        name.IsNullable.ShouldBeFalse();
        name.GetMaxLength().ShouldBe(DataSchemeConstants.EmailAddressTypeNameMaxLength);
    }

    [Fact]
    public void ConfigureMakesDescriptionRequiredWithMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var description = entityType.FindProperty(nameof(EmailAddressType.Description)).ShouldNotBeNull();
        description.IsNullable.ShouldBeFalse();
        description.GetMaxLength().ShouldBe(DataSchemeConstants.EmailAddressTypeDescriptionMaxLength);
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
    public void ConfigureRegistersOnlyEmailAddressTypeInTheModel()
    {
        // Arrange & Act
        using var context = new TestDbContext();

        // Assert
        context.Model.GetEntityTypes().ShouldHaveSingleItem().ClrType.ShouldBe(typeof(EmailAddressType));
    }

    [Fact]
    public void ConfigureWithNullBuilderThrows()
    {
        // Arrange
        var configuration = new EmailAddressTypeConfiguration();

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
            modelBuilder.ApplyConfiguration(new EmailAddressTypeConfiguration());
    }
}
