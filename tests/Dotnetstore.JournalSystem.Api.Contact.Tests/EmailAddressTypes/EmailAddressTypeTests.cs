using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddressTypes;

public sealed class EmailAddressTypeTests
{
    private static readonly Guid EmailAddressTypeId = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    [Fact]
    public void ConstructorSetsAllValues()
    {
        // Arrange & Act
        var emailAddressType = EmailAddressTypeTestData.Create(EmailAddressTypeId, "Administrator", "Can do everything");

        // Assert
        emailAddressType.Id.ShouldBe(EmailAddressTypeId);
        emailAddressType.Name.ShouldBe("Administrator");
        emailAddressType.Description.ShouldBe("Can do everything");
        emailAddressType.CreatedDate.ShouldBe(EmailAddressTypeTestData.CreatedDate);
    }

    [Fact]
    public void ConstructorSetsAuditValues()
    {
        // Arrange
        var user = Guid.NewGuid();
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var updated = created.AddDays(1);
        var deleted = created.AddDays(2);

        // Act
        var emailAddressType = new EmailAddressType(EmailAddressTypeId, "Administrator", "Description", created, user, updated, user, deleted, user, true, true, true);

        // Assert
        emailAddressType.CreatedBy.ShouldBe(user);
        emailAddressType.LastUpdatedDate.ShouldBe(updated);
        emailAddressType.LastUpdatedBy.ShouldBe(user);
        emailAddressType.DeletedDate.ShouldBe(deleted);
        emailAddressType.DeletedBy.ShouldBe(user);
        emailAddressType.IsDeleted.ShouldBeTrue();
        emailAddressType.IsSystem.ShouldBeTrue();
        emailAddressType.IsGdpr.ShouldBeTrue();
    }

    [Fact]
    public void ConstructorIsAnAuditableAndNotACompany()
    {
        // Arrange & Act
        var emailAddressType = EmailAddressTypeTestData.Create(EmailAddressTypeId, "Administrator");

        // Assert
        emailAddressType.ShouldBeAssignableTo<Auditable>();
        emailAddressType.ShouldNotBeAssignableTo<Company>();
    }

    [Fact]
    public void DifferentConstructorValuesProduceIndependentEmailAddressTypes()
    {
        // Arrange & Act
        var first = EmailAddressTypeTestData.Create(EmailAddressTypeId, "First", "One");
        var second = EmailAddressTypeTestData.Create(Guid.NewGuid(), "Second", "Two");

        // Assert
        second.Id.ShouldNotBe(first.Id);
        second.Name.ShouldNotBe(first.Name);
        second.Description.ShouldNotBe(first.Description);
    }
}
