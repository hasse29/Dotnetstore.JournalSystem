namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;

/// <summary>Someone who can sign in through the fake identity provider.</summary>
internal sealed record FakePerson(string ObjectId, string DisplayName, string UserName)
{
    internal static FakePerson CreateUnknown()
    {
        var suffix = Guid.NewGuid().ToString("N");
        return new FakePerson(Guid.NewGuid().ToString("D"), $"Nora Newcomer {suffix[..6]}", $"nora.{suffix[..8]}@example.com");
    }
}
