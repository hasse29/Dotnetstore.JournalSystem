using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Companies;

public sealed class IndexModel(IOwnCompanyClient ownCompanies) : PageModel
{
    public IReadOnlyList<OwnCompanyResponse> Companies { get; private set; } = [];

    public bool Unavailable { get; private set; }

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
}
