using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies.Images;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.OwnCompanies.Images;

public sealed class OwnCompanyImageRepositoryTests : IDisposable
{
    private static readonly DateTime Created = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

    private readonly OrganizationTestDatabase _database;
    private readonly IOwnCompanyImageRepository _repository;

    public OwnCompanyImageRepositoryTests(SqlServerFixture fixture)
    {
        _database = new OrganizationTestDatabase(fixture);
        _repository = new OwnCompanyImageRepository(_database.Context);
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    public void Dispose() => _database.Dispose();

    private async Task<Guid> SeedCompanyAsync(string name = "Pictured AB")
    {
        var company = OwnCompanyTestData.Create(Guid.NewGuid(), name, null, null, null, null, null);
        _database.Context.OwnCompanies.Add(company);
        await _database.Context.SaveChangesAsync(Token);
        _database.Context.ChangeTracker.Clear();
        return company.Id;
    }

    private async Task<OwnCompanyImage> SeedImageAsync(Guid companyId, bool isLogo = false, int minutes = 0)
    {
        var image = new OwnCompanyImage
        {
            Id = Guid.NewGuid(),
            OwnCompanyId = companyId,
            IsLogo = isLogo,
            FileName = "picture.png",
            ContentType = "image/png",
            SizeBytes = 3,
            Content = [1, 2, 3],
            CreatedDate = Created.AddMinutes(minutes)
        };
        _database.Context.OwnCompanyImages.Add(image);
        await _database.Context.SaveChangesAsync(Token);
        _database.Context.ChangeTracker.Clear();
        return image;
    }

    [Fact]
    public async Task ListAsyncReturnsTheCompanysImagesOldestFirst()
    {
        // Arrange
        var companyId = await SeedCompanyAsync();
        var newer = await SeedImageAsync(companyId, minutes: 2);
        var older = await SeedImageAsync(companyId, isLogo: true, minutes: 1);
        await SeedImageAsync(await SeedCompanyAsync("Other AB"));

        // Act
        var result = await _repository.ListAsync(companyId, Token);

        // Assert
        result.Select(x => x.Id).ShouldBe([older.Id, newer.Id]);
        result[0].IsLogo.ShouldBeTrue();
        result[0].SizeBytes.ShouldBe(3);
        _database.Context.ChangeTracker.Entries().ShouldBeEmpty();
    }

    [Fact]
    public async Task ListAsyncReturnsEmptyForACompanyWithoutImages()
    {
        // Arrange
        await SeedImageAsync(await SeedCompanyAsync("Other AB"));

        // Act
        var result = await _repository.ListAsync(await SeedCompanyAsync(), Token);

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task FindAsyncReturnsTheImageWithItsContent()
    {
        // Arrange
        var companyId = await SeedCompanyAsync();
        var image = await SeedImageAsync(companyId);

        // Act
        var result = await _repository.FindAsync(companyId, image.Id, Token);

        // Assert
        result.ShouldNotBeNull().Content.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public async Task FindAsyncDoesNotReturnAnotherCompanysImage()
    {
        // Arrange
        var image = await SeedImageAsync(await SeedCompanyAsync());
        var otherCompanyId = await SeedCompanyAsync("Other AB");

        // Act
        var result = await _repository.FindAsync(otherCompanyId, image.Id, Token);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task FindLogoAsyncReturnsTheLogoRatherThanAPhoto()
    {
        // Arrange
        var companyId = await SeedCompanyAsync();
        await SeedImageAsync(companyId);
        var logo = await SeedImageAsync(companyId, isLogo: true);

        // Act
        var result = await _repository.FindLogoAsync(companyId, Token);

        // Assert
        result.ShouldNotBeNull().Id.ShouldBe(logo.Id);
    }

    [Fact]
    public async Task FindLogoAsyncReturnsNullWhenThereAreOnlyPhotos()
    {
        // Arrange
        var companyId = await SeedCompanyAsync();
        await SeedImageAsync(companyId);

        // Act
        var result = await _repository.FindLogoAsync(companyId, Token);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task CountPhotosAsyncCountsOnlyTheCompanysPhotos()
    {
        // Arrange
        var companyId = await SeedCompanyAsync();
        await SeedImageAsync(companyId);
        await SeedImageAsync(companyId);
        await SeedImageAsync(companyId, isLogo: true);
        await SeedImageAsync(await SeedCompanyAsync("Other AB"));

        // Act
        var result = await _repository.CountPhotosAsync(companyId, Token);

        // Assert
        result.ShouldBe(2);
    }

    [Fact]
    public async Task CountPhotosAsyncIsZeroWhenThereIsOnlyALogo()
    {
        // Arrange
        var companyId = await SeedCompanyAsync();
        await SeedImageAsync(companyId, isLogo: true);

        // Act
        var result = await _repository.CountPhotosAsync(companyId, Token);

        // Assert
        result.ShouldBe(0);
    }
}
