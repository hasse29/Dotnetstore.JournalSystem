namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.E2ETests.Fixtures;

/// <summary>
/// Tests that sign in through the fake identity provider share its one-shot switches (next person, next error),
/// so every class that uses them joins this collection and runs one test at a time.
/// </summary>
[CollectionDefinition(Name)]
public sealed class MicrosoftSignInSerial
{
    public const string Name = "Microsoft sign-in";
}
