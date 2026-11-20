namespace Dotnetstore.JournalSystem.Shared.Clients;

public sealed class JournalApiClientOptions
{
    public const string SectionName = "JournalApi";

    /// <summary>
    /// Absolute address of the API host, for example <c>https://localhost:7237</c> or an Aspire service-discovery
    /// address such as <c>https+http://journalsystem-api</c>. Endpoint paths already start with <c>/api</c>, so a
    /// path in this address is ignored.
    /// </summary>
    public Uri? BaseAddress { get; set; }
}
