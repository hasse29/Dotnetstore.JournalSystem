using System.Net;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Companies;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Pages.Companies;

public sealed class IndexModelTests
{
    private static ApiResult Conflict(string detail) =>
        ApiResult.Create(HttpStatusCode.Conflict, new ApiProblem(null, detail, new Dictionary<string, string[]>()));

    private static FakeOwnCompanyClient Listing(HttpStatusCode status, params OwnCompanyResponse[] companies) =>
        new() { List = () => Task.FromResult(ApiResult.Create<IReadOnlyList<OwnCompanyResponse>>(status, companies)) };

    [Fact]
    public async Task ListsAllCompaniesSortedByName()
    {
        // Arrange
        var model = new IndexModel(Listing(HttpStatusCode.OK, FakeOwnCompanyClient.Company("Zeta AB"), FakeOwnCompanyClient.Company("alpha AB"), FakeOwnCompanyClient.Company("Beta AB")));

        // Act
        var result = await model.OnGetAsync(TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeOfType<PageResult>();
        model.Unavailable.ShouldBeFalse();
        model.Companies.Select(x => x.Name).ShouldBe(["alpha AB", "Beta AB", "Zeta AB"]);
    }

    [Fact]
    public async Task ShowsAnEmptyListWhenThereAreNoCompanies()
    {
        // Arrange
        var model = new IndexModel(Listing(HttpStatusCode.OK));

        // Act
        await model.OnGetAsync(TestContext.Current.CancellationToken);

        // Assert
        model.Unavailable.ShouldBeFalse();
        model.Companies.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task ReportsUnavailableWhenTheApiRefuses(HttpStatusCode status)
    {
        // Arrange
        var model = new IndexModel(Listing(status));

        // Act
        await model.OnGetAsync(TestContext.Current.CancellationToken);

        // Assert
        model.Unavailable.ShouldBeTrue();
        model.Companies.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReportsUnavailableWhenTheApiCannotBeReached()
    {
        // Arrange
        var model = new IndexModel(new FakeOwnCompanyClient { List = () => throw new HttpRequestException("down") });

        // Act
        await model.OnGetAsync(TestContext.Current.CancellationToken);

        // Assert
        model.Unavailable.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateSendsTheTrimmedValuesAndReportsSuccess()
    {
        // Arrange
        var client = new FakeOwnCompanyClient();
        var model = new IndexModel(client);
        var input = new CompanyInput { Name = "  New AB ", Description = " ", WebsiteUrl = "https://example.com", TaxId = " SE1 " };

        // Act
        var result = await model.OnPostCreateAsync(input, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeOfType<RedirectToPageResult>();
        model.Message.ShouldBe("Company created.");
        var sent = client.Sent.ShouldNotBeNull();
        sent.Name.ShouldBe("New AB");
        sent.Description.ShouldBeNull();
        sent.TaxId.ShouldBe("SE1");
        sent.WebsiteUrl.ShouldBe(new Uri("https://example.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public async Task CreateRejectsAMissingNameWithoutCallingTheApi(string? name)
    {
        // Arrange
        var client = new FakeOwnCompanyClient();
        var model = new IndexModel(client);

        // Act
        await model.OnPostCreateAsync(new CompanyInput { Name = name }, TestContext.Current.CancellationToken);

        // Assert
        model.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        client.Sent.ShouldBeNull();
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("ftp://example.com")]
    [InlineData("javascript:alert(1)")]
    public async Task CreateRejectsAddressesThatAreNotHttp(string website)
    {
        // Arrange
        var client = new FakeOwnCompanyClient();
        var model = new IndexModel(client);

        // Act
        await model.OnPostCreateAsync(new CompanyInput { Name = "New AB", WebsiteUrl = website }, TestContext.Current.CancellationToken);

        // Assert
        model.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        client.Sent.ShouldBeNull();
    }

    [Fact]
    public async Task CreateShowsTheApiProblemWhenItRefuses()
    {
        // Arrange
        var problem = new ApiProblem(null, null, new Dictionary<string, string[]> { ["Name"] = ["Name is required."] });
        var model = new IndexModel(new FakeOwnCompanyClient
        {
            Write = () => ApiResult.Create<OwnCompanyResponse>(HttpStatusCode.BadRequest, null, problem)
        });

        // Act
        await model.OnPostCreateAsync(new CompanyInput { Name = "x" }, TestContext.Current.CancellationToken);

        // Assert
        model.Message.ShouldBeNull();
        model.ErrorMessage.ShouldBe("Name is required.");
    }

    [Fact]
    public async Task UpdateReportsSuccess()
    {
        // Arrange
        var client = new FakeOwnCompanyClient();
        var model = new IndexModel(client);

        // Act
        await model.OnPostUpdateAsync(Guid.NewGuid(), new CompanyInput { Name = "Renamed AB" }, TestContext.Current.CancellationToken);

        // Assert
        model.Message.ShouldBe("Company updated.");
        client.Sent.ShouldNotBeNull().Name.ShouldBe("Renamed AB");
    }

    [Fact]
    public async Task UpdateReportsACompanyThatNoLongerExists()
    {
        // Arrange
        var model = new IndexModel(new FakeOwnCompanyClient { Write = () => FakeOwnCompanyClient.Saved(HttpStatusCode.NotFound) });

        // Act
        await model.OnPostUpdateAsync(Guid.NewGuid(), new CompanyInput { Name = "Renamed AB" }, TestContext.Current.CancellationToken);

        // Assert
        model.ErrorMessage.ShouldBe("That item no longer exists.");
    }

    [Fact]
    public async Task DeleteRemovesTheCompany()
    {
        // Arrange
        var client = new FakeOwnCompanyClient();
        var model = new IndexModel(client);
        var id = Guid.NewGuid();

        // Act
        var result = await model.OnPostDeleteAsync(id, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeOfType<RedirectToPageResult>();
        client.DeletedId.ShouldBe(id);
        model.Message.ShouldBe("Company deleted.");
    }

    [Fact]
    public async Task DeleteShowsTheReasonWhenTheApiRefusesASystemCompany()
    {
        // Arrange
        var model = new IndexModel(new FakeOwnCompanyClient { Remove = () => Conflict("System companies cannot be deleted.") });

        // Act
        await model.OnPostDeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        model.Message.ShouldBeNull();
        model.ErrorMessage.ShouldBe("System companies cannot be deleted.");
    }

    [Fact]
    public async Task DeleteTellsNonAdministratorsTheyAreNotAllowed()
    {
        // Arrange
        var model = new IndexModel(new FakeOwnCompanyClient { Remove = () => ApiResult.Create(HttpStatusCode.Forbidden) });

        // Act
        await model.OnPostDeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        model.ErrorMessage.ShouldBe("Only administrators can change companies.");
    }

    [Fact]
    public async Task DeleteReportsAnUnreachableApi()
    {
        // Arrange
        var model = new IndexModel(new FakeOwnCompanyClient { Remove = () => throw new HttpRequestException("down") });

        // Act
        await model.OnPostDeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        model.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        model.Message.ShouldBeNull();
    }
}
