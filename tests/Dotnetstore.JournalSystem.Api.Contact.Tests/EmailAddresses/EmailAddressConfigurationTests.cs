using Dotnetstore.JournalSystem.Api.SharedKernel.Security;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;

public sealed class EmailAddressConfigurationTests
{
    private const string ExpectedTableName = "EmailAddress";
    private const string ExpectedSchema = "Contact";

    private static IEntityType BuildEntityType()
    {
        using var context = new TestDbContext();
        return context.Model.FindEntityType(typeof(EmailAddress)).ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureMapsEmailAddressToTableNamedEmailAddress()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        entityType.GetTableName().ShouldBe(ExpectedTableName);
        entityType.GetTableName().ShouldNotBe("EmailAddressType");
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
        entityType.FindPrimaryKey().ShouldNotBeNull().Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(EmailAddress.Id));
    }

    [Fact]
    public void ConfigureMakesAddressRequiredWithEncryptedMaxLength()
    {
        // Arrange & Act
        var entityType = BuildEntityType();

        // Assert
        var property = entityType.FindProperty(nameof(EmailAddress.Address)).ShouldNotBeNull();
        property.IsNullable.ShouldBeFalse();
        property.GetMaxLength().ShouldBe(FieldEncryption.EncryptedMaxLength(DataSchemeConstants.EmailAddressMaxLength));
    }

    [Theory]
    [InlineData(nameof(EmailAddress.EmailAddressTypeId))]
    [InlineData(nameof(EmailAddress.ObjectId))]
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
    public void ConfigureRegistersOnlyEmailAddressInTheModel()
    {
        // Arrange & Act
        using var context = new TestDbContext();

        // Assert
        context.Model.GetEntityTypes().ShouldHaveSingleItem().ClrType.ShouldBe(typeof(EmailAddress));
    }

    [Fact]
    public void ConfigureWithNullBuilderThrows()
    {
        // Arrange
        var configuration = new EmailAddressConfiguration();

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
            modelBuilder.ApplyConfiguration(new EmailAddressConfiguration());
    }
}
