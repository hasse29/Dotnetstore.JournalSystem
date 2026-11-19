using Dotnetstore.JournalSystem.Api.Contact.PhoneNumbers;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.PhoneNumbers;

public sealed class PhoneNumberConfigurationTests
{
    private const string ExpectedTableName = "PhoneNumber";
    private const string ExpectedSchema = "Contact";

    private static IEntityType BuildEntityType()
    {
        using var context = new TestDbContext();
        return context.Model.FindEntityType(typeof(PhoneNumber)).ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureMapsPhoneNumberToTableNamedPhoneNumber()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.GetTableName().ShouldBe(ExpectedTableName);
        entityType.GetTableName().ShouldNotBe("PhoneNumberType");
    }

    [Fact]
    public void ConfigurePlacesTableInContactSchema()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.GetSchema().ShouldBe(ExpectedSchema);
        entityType.GetSchema().ShouldNotBe("dbo");
        entityType.GetSchema().ShouldNotBe("Organization");
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
        entityType.FindPrimaryKey().ShouldNotBeNull().Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(PhoneNumber.Id));
    }

    [Fact]
    public void ConfigureMakesAddressRequiredWithMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var property = entityType.FindProperty(nameof(PhoneNumber.Number)).ShouldNotBeNull();
        property.IsNullable.ShouldBeFalse();
        property.GetMaxLength().ShouldBe(DataSchemeConstants.PhoneNumberMaxLength);
    }

    [Theory]
    [InlineData(nameof(PhoneNumber.PhoneNumberTypeId))]
    [InlineData(nameof(PhoneNumber.ObjectId))]
    public void ConfigureMakesReferenceIdsRequired(string propertyName)
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.FindProperty(propertyName).ShouldNotBeNull().IsNullable.ShouldBeFalse();
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
    public void ConfigureRegistersOnlyPhoneNumberInTheModel()
    {
        // Arrange & Act
        using var context = new TestDbContext();

        // Assert
        context.Model.GetEntityTypes().ShouldHaveSingleItem().ClrType.ShouldBe(typeof(PhoneNumber));
    }

    [Fact]
    public void ConfigureWithNullBuilderThrows()
    {
        // Arrange
        var configuration = new PhoneNumberConfiguration();

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
            modelBuilder.ApplyConfiguration(new PhoneNumberConfiguration());
    }
}
