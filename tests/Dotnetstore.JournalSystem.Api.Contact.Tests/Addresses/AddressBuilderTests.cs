using Dotnetstore.JournalSystem.Api.Contact.Addresses;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Contact.Tests.Addresses;

public sealed class AddressBuilderTests
{
    private static EditAddressRequest CreateRequest(string street1 = "Main Street 1", string city = "Stockholm")
    {
        return new EditAddressRequest(AddressTestData.AddressTypeId, AddressTestData.ObjectId, "c/o Anna", street1, "Floor 2", "12345", city, "Stockholm County", "Sweden");
    }

    [Fact]
    public void CreateWithCreateValuesBuildsAddressFromRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        // Act
        var address = AddressBuilder.Create().WithCreateValues(CreateRequest(), userId).Build();

        // Assert
        address.Id.ShouldNotBe(Guid.Empty);
        address.AddressTypeId.ShouldBe(AddressTestData.AddressTypeId);
        address.ObjectId.ShouldBe(AddressTestData.ObjectId);
        address.CO.ShouldBe("c/o Anna");
        address.Street1.ShouldBe("Main Street 1");
        address.Street2.ShouldBe("Floor 2");
        address.PostalCode.ShouldBe("12345");
        address.City.ShouldBe("Stockholm");
        address.State.ShouldBe("Stockholm County");
        address.Country.ShouldBe("Sweden");
        address.CreatedBy.ShouldBe(userId);
        address.CreatedDate.ShouldBeInRange(before, DateTime.UtcNow);
        address.LastUpdatedDate.ShouldBeNull();
        address.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void CreateWithCreateValuesGeneratesNewIdEachTime()
    {
        // Arrange & Act
        var first = AddressBuilder.Create().WithCreateValues(CreateRequest(), null).Build();
        var second = AddressBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void CreateWithCreateValuesAllowsMissingUser()
    {
        // Arrange & Act
        var address = AddressBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Assert
        address.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void CreateWithCreateValuesRejectsEmptyUserId()
    {
        // Arrange & Act
        var act = () => AddressBuilder.Create().WithCreateValues(CreateRequest(), Guid.Empty);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWithCreateValuesRejectsBlankStreet1(string street1)
    {
        // Arrange & Act
        var act = () => AddressBuilder.Create().WithCreateValues(CreateRequest(street1), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesRejectsTooLongStreet1()
    {
        // Arrange
        var street1 = new string('a', DataSchemeConstants.AddressStreetMaxLength + 1);

        // Act
        var act = () => AddressBuilder.Create().WithCreateValues(CreateRequest(street1), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void CreateWithCreateValuesAcceptsStreet1AtMaxLength()
    {
        // Arrange
        var street1 = new string('a', DataSchemeConstants.AddressStreetMaxLength);

        // Act
        var address = AddressBuilder.Create().WithCreateValues(CreateRequest(street1), null).Build();

        // Assert
        address.Street1.ShouldBe(street1);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsIdentityAndCreationData()
    {
        // Arrange
        var creator = Guid.NewGuid();
        var existing = AddressBuilder.Create().WithCreateValues(CreateRequest(), creator).Build();
        var editor = Guid.NewGuid();
        var newOwner = Guid.NewGuid();
        var request = new EditAddressRequest(AddressTestData.AddressTypeId, newOwner, null, "New Street 2", null, "54321", "Gothenburg", "Vastra Gotaland", "Sweden");
        var before = DateTime.UtcNow;

        // Act
        var updated = AddressBuilder.Update().WithUpdateValues(existing, request, editor).Build();

        // Assert
        updated.Id.ShouldBe(existing.Id);
        updated.CreatedDate.ShouldBe(existing.CreatedDate);
        updated.CreatedBy.ShouldBe(creator);
        updated.ObjectId.ShouldBe(newOwner);
        updated.CO.ShouldBeNull();
        updated.Street1.ShouldBe("New Street 2");
        updated.Street2.ShouldBeNull();
        updated.PostalCode.ShouldBe("54321");
        updated.City.ShouldBe("Gothenburg");
        updated.LastUpdatedBy.ShouldBe(editor);
        updated.LastUpdatedDate.ShouldNotBeNull().ShouldBeInRange(before, DateTime.UtcNow);
    }

    [Fact]
    public void UpdateWithUpdateValuesKeepsFlagsFromExistingAddress()
    {
        // Arrange
        var existing = AddressBuilder.Delete()
            .SetId(Guid.NewGuid())
            .SetAddressTypeId(AddressTestData.AddressTypeId)
            .SetObjectId(AddressTestData.ObjectId)
            .SetStreet1("Main Street 1")
            .SetPostalCode("12345")
            .SetCity("Stockholm")
            .SetState("Stockholm")
            .SetCountry("Sweden")
            .SetCreatedDate(AddressTestData.CreatedDate)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Act
        var updated = AddressBuilder.Update().WithUpdateValues(existing, CreateRequest("Other Street"), null).Build();

        // Assert
        updated.IsSystem.ShouldBeTrue();
        updated.IsGdpr.ShouldBeTrue();
        updated.IsDeleted.ShouldBeFalse();
    }

    [Fact]
    public void UpdateWithUpdateValuesRejectsInvalidStreet1()
    {
        // Arrange
        var existing = AddressBuilder.Create().WithCreateValues(CreateRequest(), null).Build();

        // Act
        var act = () => AddressBuilder.Update().WithUpdateValues(existing, CreateRequest(""), null);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void SetIdAndReferenceIdsRejectEmptyGuid()
    {
        // Arrange
        var builder = AddressBuilder.Delete();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.SetId(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetAddressTypeId(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetObjectId(Guid.Empty));
    }

    [Fact]
    public void SetCreatedDateRejectsDefaultDate()
    {
        // Arrange
        var builder = AddressBuilder.Delete();

        // Act
        var act = () => builder.SetCreatedDate(default);

        // Assert
        Should.Throw<ArgumentException>(act);
    }

    [Fact]
    public void RequiredTextSettersRejectBlankAndTooLongValues()
    {
        // Arrange
        var builder = AddressBuilder.Delete();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.SetPostalCode(" "));
        Should.Throw<ArgumentException>(() => builder.SetPostalCode(new string('1', DataSchemeConstants.AddressPostalCodeMaxLength + 1)));
        Should.Throw<ArgumentException>(() => builder.SetCity(" "));
        Should.Throw<ArgumentException>(() => builder.SetCity(new string('c', DataSchemeConstants.AddressCityMaxLength + 1)));
        Should.Throw<ArgumentException>(() => builder.SetCountry(" "));
        Should.Throw<ArgumentException>(() => builder.SetCountry(new string('c', DataSchemeConstants.AddressCountryMaxLength + 1)));
    }

    [Fact]
    public void RequiredTextSettersAcceptValuesAtMaxLength()
    {
        // Arrange
        var builder = AddressBuilder.Delete();

        // Act
        var act = () => builder
            .SetPostalCode(new string('1', DataSchemeConstants.AddressPostalCodeMaxLength))
            .SetCity(new string('c', DataSchemeConstants.AddressCityMaxLength))
            .SetState(new string('s', DataSchemeConstants.AddressStateMaxLength))
            .SetCountry(new string('c', DataSchemeConstants.AddressCountryMaxLength))
            .SetStreet2(new string('s', DataSchemeConstants.AddressStreetMaxLength));

        // Assert
        Should.NotThrow(act);
    }

    [Fact]
    public void SetStateRejectsNullAndTooLongButAcceptsEmpty()
    {
        // Arrange
        var builder = AddressBuilder.Delete();

        // Act
        builder.SetState(string.Empty);

        // Assert
        Should.Throw<ArgumentNullException>(() => builder.SetState(null!));
        Should.Throw<ArgumentException>(() => builder.SetState(new string('s', DataSchemeConstants.AddressStateMaxLength + 1)));
    }

    [Fact]
    public void SetCOAcceptsNullAndBlankButRejectsTooLongAndAcceptsMaxLength()
    {
        // Arrange
        var builder = AddressBuilder.Delete();

        // Act
        builder.SetCO(null).SetCO("  ").SetCO(new string('o', DataSchemeConstants.AddressCOMaxLength));

        // Assert
        Should.Throw<ArgumentException>(() => builder.SetCO(new string('o', DataSchemeConstants.AddressCOMaxLength + 1)));
    }

    [Fact]
    public void SetStreet2AcceptsNullAndBlankButRejectsTooLong()
    {
        // Arrange
        var builder = AddressBuilder.Delete();

        // Act
        builder.SetStreet2(null).SetStreet2("  ");

        // Assert
        Should.Throw<ArgumentException>(() => builder.SetStreet2(new string('s', DataSchemeConstants.AddressStreetMaxLength + 1)));
    }

    [Fact]
    public void AuditSettersRejectEmptyUserGuids()
    {
        // Arrange
        var builder = AddressBuilder.Delete();

        // Act & Assert
        Should.Throw<ArgumentException>(() => builder.SetCreatedBy(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetLastUpdatedBy(Guid.Empty));
        Should.Throw<ArgumentException>(() => builder.SetDeletedBy(Guid.Empty));
    }

    [Fact]
    public void BuildCarriesAllSetValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = Guid.NewGuid();
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var deleted = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var address = AddressBuilder.Delete()
            .SetId(id)
            .SetStreet1("Main Street 1")
            .SetCreatedDate(created)
            .SetCreatedBy(user)
            .SetDeletedDate(deleted)
            .SetDeletedBy(user)
            .SetIsDeleted(true)
            .SetIsSystem(true)
            .SetIsGdpr(true)
            .Build();

        // Assert
        address.Id.ShouldBe(id);
        address.Street1.ShouldBe("Main Street 1");
        address.CreatedDate.ShouldBe(created);
        address.DeletedDate.ShouldBe(deleted);
        address.DeletedBy.ShouldBe(user);
        address.IsDeleted.ShouldBeTrue();
        address.IsSystem.ShouldBeTrue();
        address.IsGdpr.ShouldBeTrue();
    }
}
