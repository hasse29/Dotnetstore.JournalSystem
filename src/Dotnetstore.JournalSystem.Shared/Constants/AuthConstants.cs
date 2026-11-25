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

    /// <summary>
    /// A signed-in caller who may use the application. A Microsoft sign-in that an administrator has not approved, or
    /// whose account is locked, is authenticated but has no application account.
    /// </summary>
    public const string Approved = "Approved";

    /// <summary>Any valid sign-in, approved or not. Only for what a person needs before they are approved.</summary>
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
