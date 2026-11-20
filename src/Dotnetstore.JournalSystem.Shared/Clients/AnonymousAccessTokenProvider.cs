namespace Dotnetstore.JournalSystem.Shared.Clients;

internal sealed class AnonymousAccessTokenProvider : IAccessTokenProvider
{
    ValueTask<string?> IAccessTokenProvider.GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<string?>(null);
    }
}
