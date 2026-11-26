using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Roles;

public sealed class RoleInput
{
    public string? Name { get; set; }

    public string? Description { get; set; }
}

public sealed class IndexModel(IRoleClient roles) : PageModel
{
    private const string Forbidden = "Only administrators can change roles.";

    public IReadOnlyList<RoleResponse> Roles { get; private set; } = [];

    public bool Unavailable { get; private set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await roles.GetAllAsync(cancellationToken).ConfigureAwait(false);

            if (result.IsSuccess && result.Value is not null)
            {
                Roles = result.Value.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
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

    public async Task<IActionResult> OnPostCreateAsync(RoleInput input, CancellationToken cancellationToken)
    {
        var request = ToRequest(input, out var error);
        if (request is null)
        {
            ErrorMessage = error;
            return RedirectToPage();
        }

        return await DecideAsync(
                async () => await roles.CreateAsync(request, cancellationToken).ConfigureAwait(false),
                "Role created.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid id, RoleInput input, CancellationToken cancellationToken)
    {
        var request = ToRequest(input, out var error);
        if (request is null)
        {
            ErrorMessage = error;
            return RedirectToPage();
        }

        return await DecideAsync(
                async () => await roles.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false),
                "Role updated.")
            .ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken cancellationToken) =>
        await DecideAsync(
                async () => await roles.DeleteAsync(id, cancellationToken).ConfigureAwait(false),
                "Role deleted.")
            .ConfigureAwait(false);

    private static EditRoleRequest? ToRequest(RoleInput input, out string? error)
    {
        ArgumentNullException.ThrowIfNull(input);

        var name = input.Name?.Trim() ?? string.Empty;
        var description = input.Description?.Trim() ?? string.Empty;

        error = name switch
        {
            "" => "Name is required.",
            { Length: > DataSchemeConstants.RoleNameMaxLength } =>
                $"Name must be at most {DataSchemeConstants.RoleNameMaxLength} characters.",
            _ when description.Length > DataSchemeConstants.RoleDescriptionMaxLength =>
                $"Description must be at most {DataSchemeConstants.RoleDescriptionMaxLength} characters.",
            _ => null
        };

        return error is null ? new EditRoleRequest(name, description) : null;
    }

    private async Task<IActionResult> DecideAsync(Func<Task<ApiResult>> call, string success)
    {
        var outcome = await ApiAction.RunAsync(call, success, Forbidden).ConfigureAwait(false);
        Message = outcome.Message;
        ErrorMessage = outcome.Error;

        return RedirectToPage();
    }
}
