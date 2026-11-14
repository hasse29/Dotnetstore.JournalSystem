namespace Dotnetstore.JournalSystem.Host.Logging;

internal static partial class SecurityLog
{
    [LoggerMessage(EventId = 1001, Level = LogLevel.Warning, Message = "Rate limit exceeded for {Method} {Path}")]
    internal static partial void RateLimitExceeded(ILogger logger, string method, string path);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "Access denied with status {StatusCode} for {Method} {Path}")]
    internal static partial void AccessDenied(ILogger logger, int statusCode, string method, string path);
}
