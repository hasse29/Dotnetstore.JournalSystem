using System.Net;
using Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Companies;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Shouldly;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Pages.Companies;

public sealed class IndexModelTests
{
    private static OwnCompanyResponse Company(string name) => new(Guid.NewGuid(), name, null, null, null, null, null);

    private sealed class FakeOwnCompanyClient(Func<Task<ApiResult<IReadOnlyList<OwnCompanyResponse>>>> reply) : IOwnCompanyClient
    {
        public Task<ApiResult<IReadOnlyList<OwnCompanyResponse>>> GetAllAsync(CancellationToken cancellationToken = default) => reply();

        public Task<ApiResult<OwnCompanyResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ApiResult<OwnCompanyResponse>> CreateAsync(EditOwnCompanyRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ApiResult<OwnCompanyResponse>> UpdateAsync(Guid id, EditOwnCompanyRequest request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<ApiResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private static FakeOwnCompanyClient Replying(HttpStatusCode status, params OwnCompanyResponse[] companies) =>
        new(() => Task.FromResult(ApiResult.Create<IReadOnlyList<OwnCompanyResponse>>(status, companies)));

    [Fact]
    public async Task ListsAllCompaniesSortedByName()
    {
        // Arrange
        var model = new IndexModel(Replying(HttpStatusCode.OK, Company("Zeta AB"), Company("alpha AB"), Company("Beta AB")));

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
        var model = new IndexModel(Replying(HttpStatusCode.OK));

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
        var model = new IndexModel(Replying(status));

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
        var model = new IndexModel(new FakeOwnCompanyClient(() => throw new HttpRequestException("down")));

        // Act
        await model.OnGetAsync(TestContext.Current.CancellationToken);

        // Assert
        model.Unavailable.ShouldBeTrue();
    }
}
