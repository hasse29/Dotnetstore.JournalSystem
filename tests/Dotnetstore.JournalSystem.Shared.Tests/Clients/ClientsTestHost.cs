using System.Net;
using System.Text;
using Dotnetstore.JournalSystem.Shared.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dotnetstore.JournalSystem.Shared.Tests.Clients;

internal sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = [];

    public List<string?> Bodies { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);
        Bodies.Add(request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken));
        return respond(request);
    }

    public static HttpResponseMessage Json(HttpStatusCode status, string json, string mediaType = "application/json") =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, mediaType) };

    public static HttpResponseMessage Empty(HttpStatusCode status) => new(status);
}

internal static class ClientsTestHost
{
    public const string BaseAddress = "https://api.test";

    public static ServiceProvider Build(
        StubHandler handler,
        Action<IServiceCollection>? configure = null,
        string? baseAddress = BaseAddress)
    {
        var settings = new Dictionary<string, string?>();
        if (baseAddress is not null)
        {
            settings["JournalApi:BaseAddress"] = baseAddress;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        configure?.Invoke(services);
        services.AddJournalApiClients(configuration);
        services.ConfigureHttpClientDefaults(builder =>
            builder.ConfigurePrimaryHttpMessageHandler(() => handler));

        return services.BuildServiceProvider(validateScopes: true);
    }
}
