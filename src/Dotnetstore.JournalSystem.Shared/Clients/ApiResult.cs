using System.Net;

namespace Dotnetstore.JournalSystem.Shared.Clients;

/// <summary>
/// The outcome of one API call. HTTP error statuses are returned here, not thrown, so the caller can show them.
/// Network failures, timeouts and cancellation are still thrown as the usual exceptions.
/// </summary>
public class ApiResult
{
    internal ApiResult(HttpStatusCode statusCode, ApiProblem? problem)
    {
        StatusCode = statusCode;
        Problem = problem ?? ApiProblem.Empty;
    }

    public HttpStatusCode StatusCode { get; }

    public bool IsSuccess => (int)StatusCode is >= 200 and < 300;

    public ApiProblem Problem { get; }

    public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;

    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;

    public bool IsForbidden => StatusCode == HttpStatusCode.Forbidden;

    public bool IsConflict => StatusCode == HttpStatusCode.Conflict;

    public bool IsValidationError => StatusCode == HttpStatusCode.BadRequest && Problem.Errors.Count > 0;

    internal static ApiResult Create(HttpStatusCode statusCode, ApiProblem? problem = null) => new(statusCode, problem);

    internal static ApiResult<T> Create<T>(HttpStatusCode statusCode, T? value, ApiProblem? problem = null) =>
        new(statusCode, value, problem);
}

/// <summary>An <see cref="ApiResult"/> that also carries the response body when the call succeeded.</summary>
public sealed class ApiResult<T> : ApiResult
{
    internal ApiResult(HttpStatusCode statusCode, T? value, ApiProblem? problem)
        : base(statusCode, problem)
    {
        Value = value;
    }

    /// <summary>The response body. Only meaningful when <see cref="ApiResult.IsSuccess"/> is true.</summary>
    public T? Value { get; }
}
