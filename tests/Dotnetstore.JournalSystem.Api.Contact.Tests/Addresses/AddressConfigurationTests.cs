using Dotnetstore.JournalSystem.Api.SharedKernel.Security;
using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Addresses;

public sealed class AddressConfigurationTests
{
    private const string ExpectedTableName = "Address";
    private const string ExpectedSchema = "Contact";

    private static IEntityType BuildEntityType()
    {
        using var context = new TestDbContext();
        return context.Model.FindEntityType(typeof(Address)).ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureMapsAddressToTableNamedAddress()
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
        var key = entityType.FindPrimaryKey().ShouldNotBeNull();
        key.Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(Address.Id));
    }

    [Theory]
    [InlineData(nameof(Address.Street1), DataSchemeConstants.AddressStreetMaxLength)]
    [InlineData(nameof(Address.PostalCode), DataSchemeConstants.AddressPostalCodeMaxLength)]
    [InlineData(nameof(Address.City), DataSchemeConstants.AddressCityMaxLength)]
    [InlineData(nameof(Address.State), DataSchemeConstants.AddressStateMaxLength)]
    [InlineData(nameof(Address.Country), DataSchemeConstants.AddressCountryMaxLength)]
    public void ConfigureMakesTextPropertiesRequiredWithEncryptedMaxLength(string propertyName, int maxLength)
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var property = entityType.FindProperty(propertyName).ShouldNotBeNull();
        property.IsNullable.ShouldBeFalse();
        property.GetMaxLength().ShouldBe(FieldEncryption.EncryptedMaxLength(maxLength));
    }

    [Fact]
    public void ConfigureMakesCOOptionalWithEncryptedMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var property = entityType.FindProperty(nameof(Address.CO)).ShouldNotBeNull();
        property.IsNullable.ShouldBeTrue();
        property.GetMaxLength().ShouldBe(FieldEncryption.EncryptedMaxLength(DataSchemeConstants.AddressCOMaxLength));
    }

    [Fact]
    public void ConfigureMakesStreet2OptionalWithEncryptedMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var property = entityType.FindProperty(nameof(Address.Street2)).ShouldNotBeNull();
        property.IsNullable.ShouldBeTrue();
        property.GetMaxLength().ShouldBe(FieldEncryption.EncryptedMaxLength(DataSchemeConstants.AddressStreetMaxLength));
    }

    [Theory]
    [InlineData(nameof(Address.AddressTypeId))]
    [InlineData(nameof(Address.ObjectId))]
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
    public void ConfigureRegistersOnlyAddressInTheModel()
    {
        // Arrange & Act
        using var context = new TestDbContext();

        // Assert
        context.Model.GetEntityTypes().ShouldHaveSingleItem().ClrType.ShouldBe(typeof(Address));
    }

    [Fact]
    public void ConfigureWithNullBuilderThrows()
    {
        // Arrange
        var configuration = new AddressConfiguration();

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
            modelBuilder.ApplyConfiguration(new AddressConfiguration());
    }
}
