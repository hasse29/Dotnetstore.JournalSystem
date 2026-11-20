using Microsoft.Extensions.Logging;

namespace Dotnetstore.JournalSystem.Api.Organization.Authentication;

internal static partial class AuthLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Login rejected for locked-out user {UserId}")]
    internal static partial void LockedOutLoginRejected(this ILogger logger, Guid userId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Failed login for user {UserId}")]
    internal static partial void LoginFailed(this ILogger logger, Guid userId);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "User {UserId} logged in")]
    internal static partial void LoginSucceeded(this ILogger logger, Guid userId);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Entra user {ExternalId} {Operation}")]
    internal static partial void EntraUserChanged(this ILogger logger, string externalId, string operation);

    [LoggerMessage(EventId = 11, Level = LogLevel.Warning, Message = "Entra {Operation} failed with status {Status} and code {Code}")]
    internal static partial void EntraOperationFailed(this ILogger logger, string operation, int? status, string? code);

    [LoggerMessage(EventId = 12, Level = LogLevel.Warning, Message = "Could not obtain a Microsoft Graph token for the signed-in user")]
    internal static partial void GraphTokenUnavailable(this ILogger logger, Exception exception);
}
