namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Authentication;

internal static partial class EntraLog
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Microsoft sign-in failed")]
    internal static partial void SignInFailed(ILogger logger, Exception? exception);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "The API rejected the signed-in user's access token (401). Token: {Summary}")]
    internal static partial void ApiRejectedToken(ILogger logger, string summary);
}
