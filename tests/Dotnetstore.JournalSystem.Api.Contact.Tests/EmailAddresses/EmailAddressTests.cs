using Dotnetstore.BaseModel.Entities;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddresses;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.EmailAddresses;

public sealed class EmailAddressTests
{
    private static readonly Guid EmailAddressId = Guid.Parse("6f9619ff-8b86-d011-b42d-00c04fc964ff");

    [Fact]
    public void ConstructorSetsAllValues()
    {
        // Arrange & Act
        var emailAddress = EmailAddressTestData.Create(EmailAddressId, "anna@example.com");

        // Assert
        emailAddress.Id.ShouldBe(EmailAddressId);
        emailAddress.EmailAddressTypeId.ShouldBe(EmailAddressTestData.EmailAddressTypeId);
        emailAddress.ObjectId.ShouldBe(EmailAddressTestData.ObjectId);
        emailAddress.Address.ShouldBe("anna@example.com");
        emailAddress.CreatedDate.ShouldBe(EmailAddressTestData.CreatedDate);
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
        var emailAddress = new EmailAddress(EmailAddressId, Guid.NewGuid(), Guid.NewGuid(), "a@b.se", created, user, updated, user, deleted, user, true, true, true);

        // Assert
        emailAddress.CreatedBy.ShouldBe(user);
        emailAddress.LastUpdatedDate.ShouldBe(updated);
        emailAddress.LastUpdatedBy.ShouldBe(user);
        emailAddress.DeletedDate.ShouldBe(deleted);
        emailAddress.DeletedBy.ShouldBe(user);
        emailAddress.IsDeleted.ShouldBeTrue();
        emailAddress.IsSystem.ShouldBeTrue();
        emailAddress.IsGdpr.ShouldBeTrue();
    }

    [Fact]
    public void ConstructorIsAnAuditableAndNotACompany()
    {
        // Arrange & Act
        var emailAddress = EmailAddressTestData.Create(EmailAddressId);

        // Assert
        emailAddress.ShouldBeAssignableTo<Auditable>();
        emailAddress.ShouldNotBeAssignableTo<Company>();
    }

    [Fact]
    public void DifferentConstructorValuesProduceIndependentEmailAddresses()
    {
        // Arrange & Act
        var first = EmailAddressTestData.Create(EmailAddressId, "first@example.com");
        var second = EmailAddressTestData.Create(Guid.NewGuid(), "second@example.com", objectId: Guid.NewGuid());

        // Assert
        second.Id.ShouldNotBe(first.Id);
        second.Address.ShouldNotBe(first.Address);
        second.ObjectId.ShouldNotBe(first.ObjectId);
    }
}
