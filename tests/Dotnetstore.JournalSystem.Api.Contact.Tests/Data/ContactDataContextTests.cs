using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.Data;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Data;

public sealed class ContactDataContextTests
{
    private static ContactDataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ContactDataContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        return new ContactDataContext(options);
    }

    [Fact]
    public void ModelContainsAddressTypeEntity()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(AddressType));

        // Assert
        entityType.ShouldNotBeNull();
    }

    [Fact]
    public void ModelContainsOnlyContactEntities()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityTypes = context.Model.GetEntityTypes().ToList();

        // Assert
        entityTypes.Select(x => x.ClrType).ShouldBe([typeof(AddressType), typeof(Address), typeof(EmailAddressType), typeof(EmailAddress)], ignoreOrder: true);
    }

    [Fact]
    public void ModelDoesNotContainAbstractBaseTypes()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var auditable = context.Model.FindEntityType(typeof(Auditable));

        // Assert
        auditable.ShouldBeNull();
    }

    [Fact]
    public void AddressTypesDbSetIsExposed()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var dbSet = context.AddressTypes;

        // Assert
        dbSet.ShouldNotBeNull();
        dbSet.EntityType.ClrType.ShouldBe(typeof(AddressType));
    }

    [Fact]
    public void ModelAppliesAddressTypeConfigurationFromAssembly()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(AddressType)).ShouldNotBeNull();

        // Assert
        entityType.GetTableName().ShouldBe("AddressType");
        entityType.GetSchema().ShouldBe("Contact");
        entityType.FindPrimaryKey().ShouldNotBeNull().Properties.ShouldHaveSingleItem().Name.ShouldBe(nameof(AddressType.Id));
    }

    [Fact]
    public void AddressesDbSetIsExposed()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var dbSet = context.Addresses;

        // Assert
        dbSet.ShouldNotBeNull();
        dbSet.EntityType.ClrType.ShouldBe(typeof(Address));
    }

    [Fact]
    public void ModelAppliesAddressConfigurationFromAssembly()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Address)).ShouldNotBeNull();

        // Assert
        entityType.GetTableName().ShouldBe("Address");
        entityType.GetSchema().ShouldBe("Contact");
    }

    [Fact]
    public void EmailAddressTypesDbSetIsExposed()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var dbSet = context.EmailAddressTypes;

        // Assert
        dbSet.ShouldNotBeNull();
        dbSet.EntityType.ClrType.ShouldBe(typeof(EmailAddressType));
    }

    [Fact]
    public void ModelAppliesEmailAddressTypeConfigurationFromAssembly()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailAddressType)).ShouldNotBeNull();

        // Assert
        entityType.GetTableName().ShouldBe("EmailAddressType");
        entityType.GetSchema().ShouldBe("Contact");
    }

    [Fact]
    public void EmailAddressesDbSetIsExposed()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var dbSet = context.EmailAddresses;

        // Assert
        dbSet.ShouldNotBeNull();
        dbSet.EntityType.ClrType.ShouldBe(typeof(EmailAddress));
    }

    [Fact]
    public void ModelAppliesEmailAddressConfigurationFromAssembly()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(EmailAddress)).ShouldNotBeNull();

        // Assert
        entityType.GetTableName().ShouldBe("EmailAddress");
        entityType.GetSchema().ShouldBe("Contact");
    }

    [Fact]
    public void ModelDoesNotUseTheOrganizationSchema()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(AddressType)).ShouldNotBeNull();

        // Assert
        entityType.GetSchema().ShouldNotBe("Organization");
        entityType.GetSchema().ShouldNotBeNull();
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
