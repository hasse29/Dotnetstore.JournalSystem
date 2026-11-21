namespace Dotnetstore.JournalSystem.Shared.Constants;

public static class AuthClaimTypes
{
    public const string Role = "role";
    public const string UserId = "user_id";
    public const string OwnCompanyId = "own_company_id";
}

public static class AuthPolicies
{
    public const string Administrator = "Administrator";
    public const string Authenticated = "Authenticated";
}

public static class RoleNames
{
    public const string Administrator = "Administrator";
}

public static class AuthSchemes
{
    public const string Journal = "Journal";
    public const string Local = "Local";
    public const string Entra = "Entra";
}
