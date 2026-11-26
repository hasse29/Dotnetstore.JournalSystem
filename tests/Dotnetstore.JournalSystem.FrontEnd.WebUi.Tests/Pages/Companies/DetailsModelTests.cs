using System.Net;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Companies;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Pages.Companies;

public sealed class DetailsModelTests
{
    private readonly Guid _id = Guid.NewGuid();
    private readonly FakeAddressClient _addresses = new();
    private readonly FakeAddressTypeClient _addressTypes = new();
    private readonly FakeEmailAddressClient _emails = new();
    private readonly FakeEmailAddressTypeClient _emailTypes = new();
    private readonly FakePhoneNumberClient _phones = new();
    private readonly FakePhoneNumberTypeClient _phoneTypes = new();
    private readonly FakeUserClient _users = new();
    private readonly FakeRoleClient _roles = new();

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private DetailsModel Create(FakeOwnCompanyClient companies, FakeCurrentUserProfile? profile = null) =>
        new(companies, _addresses, _addressTypes, _emails, _emailTypes, _phones, _phoneTypes, _users, _roles,
            profile ?? FakeCurrentUserProfile.Administrator())
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
        };

    private static FormFile File(int length)
    {
        return new FormFile(new MemoryStream(new byte[length]), 0, length, "file", "picture.png");
    }

    private static UserResponse User(string firstName, Guid companyId, Guid roleId) =>
        new(Guid.NewGuid(), firstName, null, "Person", $"{firstName}@example.com", companyId, roleId, AuthProvider.Local, false, null);

    [Fact]
    public async Task GetLoadsTheCompanyWithItsImagesContactsAndUsers()
    {
        // Arrange
        var logo = new OwnCompanyImageResponse(Guid.NewGuid(), true, "logo.png", "image/png", 3, DateTime.UtcNow);
        var photo = new OwnCompanyImageResponse(Guid.NewGuid(), false, "office.png", "image/png", 3, DateTime.UtcNow);
        var companies = new FakeOwnCompanyClient
        {
            Images = () => ApiResult.Create<IReadOnlyList<OwnCompanyImageResponse>>(HttpStatusCode.OK, [logo, photo])
        };
        var roleId = Guid.NewGuid();
        _roles.Items.Add(new RoleResponse(roleId, "Nurse", "Nurse"));
        _users.Items.Add(User("Anna", _id, roleId));
        _users.Items.Add(User("Elsewhere", Guid.NewGuid(), roleId));
        _phones.Items.Add(new PhoneNumberResponse(Guid.NewGuid(), Guid.NewGuid(), _id, "+46 70 111 11 11"));
        _phones.Items.Add(new PhoneNumberResponse(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "+46 70 222 22 22"));
        var model = Create(companies);

        // Act
        var result = await model.OnGetAsync(_id, Token);

        // Assert
        result.ShouldBeOfType<PageResult>();
        model.Company.ShouldNotBeNull();
        model.CanEdit.ShouldBeTrue();
        model.Logo.ShouldBe(logo);
        model.Photos.ShouldHaveSingleItem().ShouldBe(photo);
        model.PhoneNumbers.ShouldNotBeNull().ShouldHaveSingleItem().Number.ShouldBe("+46 70 111 11 11");
        model.Users.ShouldNotBeNull().ShouldHaveSingleItem().ShouldBe(new CompanyUser("Anna Person", "Anna@example.com", "Nurse", false));
    }

    [Fact]
    public async Task GetHidesUsersAndEditingFromNonAdministrators()
    {
        // Arrange
        _users.Items.Add(User("Anna", _id, Guid.NewGuid()));
        var model = Create(new FakeOwnCompanyClient(), FakeCurrentUserProfile.With(Permissions.OwnCompanies.View));

        // Act
        await model.OnGetAsync(_id, Token);

        // Assert
        model.CanEdit.ShouldBeFalse();
        model.Users.ShouldBeNull();
    }

    [Fact]
    public async Task GetReturnsNotFoundForAnUnknownCompany()
    {
        // Arrange
        var model = Create(new FakeOwnCompanyClient { Read = () => FakeOwnCompanyClient.Saved(HttpStatusCode.NotFound) });

        // Act
        var result = await model.OnGetAsync(_id, Token);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetShowsNoCompanyWhenTheApiCannotBeReached()
    {
        // Arrange
        var model = Create(new FakeOwnCompanyClient { Read = () => throw new HttpRequestException("down") });

        // Act
        var result = await model.OnGetAsync(_id, Token);

        // Assert
        result.ShouldBeOfType<PageResult>();
        model.Company.ShouldBeNull();
    }

    [Fact]
    public async Task AFailingSectionDoesNotTakeDownTheOthers()
    {
        // Arrange
        _addresses.Unreachable = true;
        var model = Create(new FakeOwnCompanyClient());

        // Act
        await model.OnGetAsync(_id, Token);

        // Assert
        model.Company.ShouldNotBeNull();
        model.Addresses.ShouldBeNull();
        model.EmailAddresses.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteGoesBackToTheListOnSuccess()
    {
        // Arrange
        var model = Create(new FakeOwnCompanyClient());

        // Act
        var result = await model.OnPostDeleteAsync(_id, Token);

        // Assert
        result.ShouldBeOfType<RedirectToPageResult>().PageName.ShouldBe("/Companies/Index");
        model.Message.ShouldBe("Company deleted.");
    }

    [Fact]
    public async Task DeleteStaysOnThePageWhenRefused()
    {
        // Arrange
        var problem = new ApiProblem(null, "The company still has users.", new Dictionary<string, string[]>());
        var model = Create(new FakeOwnCompanyClient { Remove = () => ApiResult.Create(HttpStatusCode.Conflict, problem) });

        // Act
        var result = await model.OnPostDeleteAsync(_id, Token);

        // Assert
        result.ShouldBeOfType<RedirectToPageResult>().PageName.ShouldBeNull();
        model.ErrorMessage.ShouldBe("The company still has users.");
    }

    [Fact]
    public async Task UpdateKeepsTheStoredLogoAddress()
    {
        // Arrange
        var companies = new FakeOwnCompanyClient();
        var model = Create(companies);

        // Act
        await model.OnPostUpdateAsync(_id, new CompanyInput { Name = "Renamed AB", LogoUrl = "https://example.com/logo.png" }, Token);

        // Assert
        companies.Sent.ShouldNotBeNull().LogoUrl.ShouldBe(new Uri("https://example.com/logo.png"));
        model.Message.ShouldBe("Company updated.");
    }

    [Fact]
    public async Task UpdateRejectsAMissingNameWithoutCallingTheApi()
    {
        // Arrange
        var companies = new FakeOwnCompanyClient();
        var model = Create(companies);

        // Act
        await model.OnPostUpdateAsync(_id, new CompanyInput(), Token);

        // Assert
        companies.Sent.ShouldBeNull();
        model.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UploadsSendTheFileToTheApi()
    {
        // Arrange
        var companies = new FakeOwnCompanyClient();
        var model = Create(companies);

        // Act
        await model.OnPostLogoAsync(_id, File(10), Token);
        await model.OnPostPhotoAsync(_id, File(20), Token);

        // Assert
        companies.UploadedLogo.ShouldNotBeNull().Content.Length.ShouldBe(10);
        companies.UploadedPhoto.ShouldNotBeNull().FileName.ShouldBe("picture.png");
        model.Message.ShouldBe("Photo added.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(OwnCompanyImageConstants.MaxSizeBytes + 1)]
    public async Task EmptyAndTooLargeUploadsAreRefusedWithoutCallingTheApi(int length)
    {
        // Arrange
        var companies = new FakeOwnCompanyClient();
        var model = Create(companies);

        // Act
        await model.OnPostLogoAsync(_id, File(length), Token);
        await model.OnPostPhotoAsync(_id, null, Token);

        // Assert
        companies.UploadedLogo.ShouldBeNull();
        companies.UploadedPhoto.ShouldBeNull();
        model.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ImageHandlerPassesOnTheImageAndLetsTheBrowserCacheIt()
    {
        // Arrange
        var model = Create(new FakeOwnCompanyClient());

        // Act
        var result = await model.OnGetImageAsync(_id, Guid.NewGuid(), Token);

        // Assert
        result.ShouldBeOfType<FileContentResult>().ContentType.ShouldBe("image/png");
        model.Response.Headers.CacheControl.ToString().ShouldBe("private, max-age=86400");
    }

    [Fact]
    public async Task ImageHandlerReturnsNotFoundForAMissingImage()
    {
        // Arrange
        var model = Create(new FakeOwnCompanyClient { Content = () => ApiResult.Create<ApiFile>(HttpStatusCode.NotFound, null) });

        // Act
        var result = await model.OnGetImageAsync(_id, Guid.NewGuid(), Token);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task SavingAnAddressWithoutAnIdCreatesItForThisCompany()
    {
        // Arrange
        var model = Create(new FakeOwnCompanyClient());
        var input = new AddressInput { TypeId = Guid.NewGuid(), Street1 = " Main Street 1 ", PostalCode = "12345", City = "Stockholm", Country = "Sweden" };

        // Act
        await model.OnPostSaveAddressAsync(_id, null, input, Token);

        // Assert
        var created = _addresses.Created.ShouldNotBeNull();
        created.ObjectId.ShouldBe(_id);
        created.Street1.ShouldBe("Main Street 1");
        created.State.ShouldBeEmpty();
        model.Message.ShouldBe("Address saved.");
    }

    [Fact]
    public async Task SavingWithAnIdUpdatesThatContact()
    {
        // Arrange
        var model = Create(new FakeOwnCompanyClient());
        var contactId = Guid.NewGuid();
        var input = new ContactValueInput { TypeId = Guid.NewGuid(), Value = "info@example.com" };

        // Act
        await model.OnPostSaveEmailAsync(_id, contactId, input, Token);

        // Assert
        _emails.Updated.ShouldNotBeNull().Id.ShouldBe(contactId);
        _emails.Created.ShouldBeNull();
    }

    [Fact]
    public async Task IncompleteContactsAreRefusedWithoutCallingTheApi()
    {
        // Arrange
        var model = Create(new FakeOwnCompanyClient());

        // Act
        await model.OnPostSaveAddressAsync(_id, null, new AddressInput { TypeId = Guid.NewGuid() }, Token);
        await model.OnPostSaveEmailAsync(_id, null, new ContactValueInput { Value = "info@example.com" }, Token);
        await model.OnPostSavePhoneAsync(_id, null, new ContactValueInput { TypeId = Guid.NewGuid(), Value = " " }, Token);

        // Assert
        _addresses.Created.ShouldBeNull();
        _emails.Created.ShouldBeNull();
        _phones.Created.ShouldBeNull();
        model.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task DeletingContactsAndImagesUsesTheGivenId()
    {
        // Arrange
        var companies = new FakeOwnCompanyClient();
        var model = Create(companies);
        var contactId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        // Act
        await model.OnPostDeletePhoneAsync(_id, contactId, Token);
        await model.OnPostDeleteImageAsync(_id, imageId, Token);

        // Assert
        _phones.DeletedId.ShouldBe(contactId);
        companies.DeletedImageId.ShouldBe(imageId);
    }

    [Fact]
    public async Task GetLeavesOutContactsWhenThePersonMayNotViewThem()
    {
        // Arrange
        _phones.Items.Add(new PhoneNumberResponse(Guid.NewGuid(), Guid.NewGuid(), _id, "+46 70 111 11 11"));
        var model = Create(new FakeOwnCompanyClient(), FakeCurrentUserProfile.With(Permissions.OwnCompanies.View));

        // Act
        await model.OnGetAsync(_id, Token);

        // Assert
        model.CanViewContacts.ShouldBeFalse();
        model.PhoneNumbers.ShouldBeNull();
    }

    [Fact]
    public async Task GetOffersOnlyWhatTheRightsAllow()
    {
        // Arrange
        var model = Create(new FakeOwnCompanyClient(), FakeCurrentUserProfile.With(
            Permissions.OwnCompanies.View,
            Permissions.ContactDetails.View,
            Permissions.ContactDetails.Create,
            Permissions.Users.View));

        // Act
        await model.OnGetAsync(_id, Token);

        // Assert
        model.CanEdit.ShouldBeFalse();
        model.CanDelete.ShouldBeFalse();
        model.CanViewContacts.ShouldBeTrue();
        model.CanAddContacts.ShouldBeTrue();
        model.CanEditContacts.ShouldBeFalse();
        model.CanDeleteContacts.ShouldBeFalse();
        model.CanViewUsers.ShouldBeTrue();
        model.Users.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetSendsThoseWhoMayNotViewTheCompanyToTheList()
    {
        // Arrange
        var companies = new FakeOwnCompanyClient { Read = () => FakeOwnCompanyClient.Saved(HttpStatusCode.Forbidden) };
        var model = Create(companies, FakeCurrentUserProfile.With());

        // Act
        var result = await model.OnGetAsync(_id, Token);

        // Assert
        result.ShouldBeOfType<RedirectToPageResult>().PageName.ShouldBe("/Companies/Index");
    }
}
