namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal interface IPersonalIdentityNumberProtector
{
    /// <summary>Encrypts a valid number for storage, bound to the user it belongs to. Null stays null.</summary>
    string? Protect(string? personalIdentityNumber, Guid userId);

    /// <summary>The masked form of a stored number, or null when none is stored. The full number never leaves this class.</summary>
    string? Mask(string? stored, Guid userId);
}
