using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies.Images;

public sealed class OwnCompanyImageServiceTests : IDisposable
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private readonly OrganizationTestDatabase _database;
    private readonly IOwnCompanyImageService _service;

    public OwnCompanyImageServiceTests(SqlServerFixture fixture)
    {
        _database = new OrganizationTestDatabase(fixture);
        _service = new OwnCompanyImageService(
            _database.UnitOfWork,
            new OwnCompanyImageRepository(_database.Context),
            TimeProvider.System);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    public void Dispose() => _database.Dispose();

    private async Task<Guid> SeedCompanyAsync()
    {
        IOwnCompanyService companies = new OwnCompanyService(_database.UnitOfWork);
        var result = await companies.CreateAsync(new EditOwnCompanyRequest("Pictured AB", null, null, null, null, null), null, Token);
        _database.Context.ChangeTracker.Clear();
        return result.Value.Id;
    }

    private Task<int> CountStoredAsync() => _database.Context.OwnCompanyImages.AsNoTracking().CountAsync(Token);

    [Fact]
    public async Task SetLogoStoresTheImageWithItsDetectedType()
    {
        // Arrange
        var companyId = await SeedCompanyAsync();
        var userId = Guid.NewGuid();

        // Act
        var result = await _service.SetLogoAsync(companyId, @"C:\pictures\logo.gif", Png, userId, Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.FileName.ShouldBe("logo.gif");
        result.Value.ContentType.ShouldBe("image/png");
        var stored = await _database.Context.OwnCompanyImages.AsNoTracking().SingleAsync(Token);
        stored.IsLogo.ShouldBeTrue();
        stored.CreatedBy.ShouldBe(userId);
        stored.Content.ShouldBe(Png);
    }

    [Theory]
    [InlineData(@"C:\pictures\logo.png", "logo.png")]
    [InlineData("/home/anna/logo.png", "logo.png")]
    [InlineData(@"..\../logo.png", "logo.png")]
    [InlineData("pictures/", "image")]
    [InlineData("   ", "image")]
    public async Task SetLogoKeepsOnlyTheFileNameOnEveryPlatform(string fileName, string expected)
    {
        // Arrange
        var companyId = await SeedCompanyAsync();

        // Act
        var result = await _service.SetLogoAsync(companyId, fileName, Png, null, Token);

        // Assert
        result.Value.FileName.ShouldBe(expected);
    }

    [Fact]
    public async Task SetLogoReplacesTheExistingLogo()
    {
        // Arrange
        var companyId = await SeedCompanyAsync();
        var first = await _service.SetLogoAsync(companyId, "a.png", Png, null, Token);
        _database.Context.ChangeTracker.Clear();

        // Act
        var second = await _service.SetLogoAsync(companyId, "b.png", Png, null, Token);

        // Assert
        second.Value.Id.ShouldNotBe(first.Value.Id);
        (await CountStoredAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task AddingToAnUnknownCompanyReturnsNotFound()
    {
        // Arrange

        // Act
        var logo = await _service.SetLogoAsync(Guid.NewGuid(), "a.png", Png, null, Token);
        var photo = await _service.AddPhotoAsync(Guid.NewGuid(), Guid.NewGuid(), "a.png", Png, null, Token);
        var list = await _service.GetAllAsync(Guid.NewGuid(), Token);

        // Assert
        logo.Status.ShouldBe(ResultStatus.NotFound);
        photo.Status.ShouldBe(ResultStatus.NotFound);
        list.Status.ShouldBe(ResultStatus.NotFound);
        (await CountStoredAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task AddingTheSamePhotoAgainStoresItOnce()
    {
        // Arrange
        var companyId = await SeedCompanyAsync();
        var imageId = Guid.NewGuid();
        await _service.AddPhotoAsync(companyId, imageId, "a.png", Png, null, Token);
        _database.Context.ChangeTracker.Clear();

        // Act
        var result = await _service.AddPhotoAsync(companyId, imageId, "a.png", Png, null, Token);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(imageId);
        (await CountStoredAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task APhotoCannotTakeTheIdOfAnotherCompanysImage()
    {
        // Arrange
        var owner = await SeedCompanyAsync();
        var other = await SeedCompanyAsync();
        var imageId = Guid.NewGuid();
        await _service.AddPhotoAsync(owner, imageId, "a.png", Png, null, Token);
        _database.Context.ChangeTracker.Clear();

        // Act
        var result = await _service.AddPhotoAsync(other, imageId, "b.png", Png, null, Token);

        // Assert
        result.Status.ShouldBe(ResultStatus.Conflict);
        (await CountStoredAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task PhotosAreLimitedPerCompanyButTheLogoDoesNotCount()
    {
        // Arrange
        var companyId = await SeedCompanyAsync();
        await _service.SetLogoAsync(companyId, "logo.png", Png, null, Token);
        for (var i = 0; i < OwnCompanyImageConstants.MaxPhotosPerCompany; i++)
        {
            (await _service.AddPhotoAsync(companyId, Guid.NewGuid(), "a.png", Png, null, Token)).IsSuccess.ShouldBeTrue();
        }

        // Act
        var result = await _service.AddPhotoAsync(companyId, Guid.NewGuid(), "one-too-many.png", Png, null, Token);

        // Assert
        result.Status.ShouldBe(ResultStatus.Conflict);
        (await CountStoredAsync()).ShouldBe(OwnCompanyImageConstants.MaxPhotosPerCompany + 1);
    }

    [Fact]
    public async Task ImagesAreOnlyReachableThroughTheirOwnCompany()
    {
        // Arrange
        var owner = await SeedCompanyAsync();
        var other = await SeedCompanyAsync();
        var imageId = Guid.NewGuid();
        await _service.AddPhotoAsync(owner, imageId, "a.png", Png, null, Token);
        _database.Context.ChangeTracker.Clear();

        // Act
        var read = await _service.GetAsync(other, imageId, Token);
        var delete = await _service.DeleteAsync(other, imageId, Token);
        var list = await _service.GetAllAsync(other, Token);

        // Assert
        read.Status.ShouldBe(ResultStatus.NotFound);
        delete.Status.ShouldBe(ResultStatus.NotFound);
        list.Value.ShouldBeEmpty();
        (await CountStoredAsync()).ShouldBe(1);
    }

    [Fact]
    public async Task DeleteRemovesTheImage()
    {
        // Arrange
        var companyId = await SeedCompanyAsync();
        var imageId = Guid.NewGuid();
        await _service.AddPhotoAsync(companyId, imageId, " ", Png, null, Token);
        _database.Context.ChangeTracker.Clear();
        var listed = await _service.GetAllAsync(companyId, Token);

        // Act
        var result = await _service.DeleteAsync(companyId, imageId, Token);

        // Assert
        listed.Value.ShouldHaveSingleItem().FileName.ShouldBe("image");
        result.IsSuccess.ShouldBeTrue();
        (await CountStoredAsync()).ShouldBe(0);
    }
}
