using Dotnetstore.JournalSystem.Shared.Clients;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages;

/// <summary>The message to show after a change: exactly one of the two is set.</summary>
internal sealed record ApiOutcome(string? Message, string? Error);

internal static class ApiAction
{
    internal static async Task<ApiOutcome> RunAsync(
        Func<Task<ApiResult>> call,
        string success,
        string forbidden = "You do not have permission to do that.")
    {
        try
        {
            var result = await call().ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return new ApiOutcome(success, null);
            }

            if (result.IsForbidden || result.IsUnauthorized)
            {
                return Failed(result.Problem.Detail ?? forbidden);
            }

            if (result.IsNotFound)
            {
                return Failed("That item no longer exists.");
            }

            return Failed(
                result.Problem.Detail
                ?? result.Problem.Errors.Values.SelectMany(x => x).FirstOrDefault()
                ?? "The change could not be saved.");
        }
        catch (HttpRequestException)
        {
            return Failed("The service is unavailable right now. Please try again.");
        }
        catch (TimeoutException)
        {
            return Failed("The service did not respond in time. Please try again.");
        }
    }

    private static ApiOutcome Failed(string error) => new(null, error);
}
