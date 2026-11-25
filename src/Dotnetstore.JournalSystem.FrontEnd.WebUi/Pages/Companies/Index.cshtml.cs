using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Companies;

public sealed class IndexModel(IOwnCompanyClient ownCompanies) : PageModel
{
    public IReadOnlyList<OwnCompanyResponse> Companies { get; private set; } = [];

    public bool Unavailable { get; private set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await ownCompanies.GetAllAsync(cancellationToken).ConfigureAwait(false);

            if (result.IsSuccess && result.Value is not null)
            {
                Companies = result.Value.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
            }
            else
            {
                Unavailable = true;
            }
        }
        catch (HttpRequestException)
        {
            Unavailable = true;
        }
        catch (TimeoutException)
        {
            Unavailable = true;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(CompanyInput input, CancellationToken cancellationToken)
    {
        var request = CompanyForm.ToRequest(input, out var error);
        if (request is null)
        {
            ErrorMessage = error;
            return RedirectToPage();
        }

        return await DecideAsync(
                async () => await ownCompanies.CreateAsync(request, cancellationToken).ConfigureAwait(false),
                "Company created.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid id, CompanyInput input, CancellationToken cancellationToken)
    {
        var request = CompanyForm.ToRequest(input, out var error);
        if (request is null)
        {
            ErrorMessage = error;
            return RedirectToPage();
        }

        return await DecideAsync(
                async () => await ownCompanies.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false),
                "Company updated.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken) =>
        await DecideAsync(
                async () => await ownCompanies.DeleteAsync(id, cancellationToken).ConfigureAwait(false),
                "Company deleted.")
            .ConfigureAwait(false);

    private async Task<IActionResult> DecideAsync(Func<Task<ApiResult>> call, string success)
    {
        var outcome = await ApiAction.RunAsync(call, success).ConfigureAwait(false);
        Message = outcome.Message;
        ErrorMessage = outcome.Error;

        return RedirectToPage();
    }
}
