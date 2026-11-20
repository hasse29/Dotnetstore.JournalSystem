using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Dotnetstore.JournalSystem.Shared.Clients;

internal abstract class ApiClientBase(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected static Uri BuildUri(string template, Guid id) =>
        new(template.Replace("{id:guid}", id.ToString("D"), StringComparison.Ordinal), UriKind.Relative);

    protected static Uri BuildUri(string template) => new(template, UriKind.Relative);

    protected async Task<ApiResult<T>> SendAsync<T>(
        HttpMethod method,
        Uri uri,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(method, uri, body, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var problem = await ReadProblemAsync(response, cancellationToken).ConfigureAwait(false);
            return ApiResult.Create<T>(response.StatusCode, default, problem);
        }

        var value = await ReadValueAsync<T>(response, cancellationToken).ConfigureAwait(false);
        return ApiResult.Create(response.StatusCode, value);
    }

    protected async Task<ApiResult> SendAsync(
        HttpMethod method,
        Uri uri,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = await SendCoreAsync(method, uri, body, cancellationToken).ConfigureAwait(false);

        var problem = response.IsSuccessStatusCode
            ? null
            : await ReadProblemAsync(response, cancellationToken).ConfigureAwait(false);

        return ApiResult.Create(response.StatusCode, problem);
    }

    private async Task<HttpResponseMessage> SendCoreAsync(
        HttpMethod method,
        Uri uri,
        object? body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, uri);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);
        }

        return await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<T?> ReadValueAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            return await JsonSerializer
                .DeserializeAsync<T>(stream, JsonOptions, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static async Task<ApiProblem> ReadProblemAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(text))
        {
            return ApiProblem.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(text);
            var root = document.RootElement;

            switch (root.ValueKind)
            {
                case JsonValueKind.String:
                    return new ApiProblem(null, root.GetString(), ApiProblem.Empty.Errors);
                case JsonValueKind.Object:
                    return ParseProblemObject(root);
                default:
                    return new ApiProblem(null, text, ApiProblem.Empty.Errors);
            }
        }
        catch (JsonException)
        {
            return new ApiProblem(null, text, ApiProblem.Empty.Errors);
        }
    }

    private static ApiProblem ParseProblemObject(JsonElement root)
    {
        var title = GetString(root, "title");
        var detail = GetString(root, "detail");
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (root.TryGetProperty("errors", out var errorsElement) && errorsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in errorsElement.EnumerateObject())
            {
                errors[property.Name] = property.Value.ValueKind == JsonValueKind.Array
                    ? [.. property.Value.EnumerateArray()
                        .Where(static item => item.ValueKind == JsonValueKind.String)
                        .Select(static item => item.GetString()!)]
                    : [];
            }
        }

        return new ApiProblem(title, detail, errors);
    }

    private static string? GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
