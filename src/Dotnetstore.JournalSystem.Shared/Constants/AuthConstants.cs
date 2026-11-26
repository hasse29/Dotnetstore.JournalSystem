namespace Dotnetstore.JournalSystem.Shared.Constants;

public static class AuthClaimTypes
{
    public const string Role = "role";
    public const string UserId = "user_id";
    public const string OwnCompanyId = "own_company_id";

    /// <summary>Added by the API for each right of the caller's role; never taken from a token.</summary>
    public const string Permission = "permission";
}

public static class AuthPolicies
{
    public const string Administrator = "Administrator";

    // Each value in Permissions is also a policy name.

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
