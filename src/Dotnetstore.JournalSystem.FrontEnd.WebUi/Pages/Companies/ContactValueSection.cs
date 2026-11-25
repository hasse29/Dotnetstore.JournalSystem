namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Pages.Companies;

public sealed record ContactValueRow(Guid Id, Guid TypeId, string Value);

/// <summary>What differs between the email and the phone sections, which otherwise render the same.</summary>
public sealed record ContactValueSection(
    DetailsModel Page,
    IReadOnlyList<ContactValueRow>? Items,
    IReadOnlyList<ContactType> Types,
    string Key,
    string Title,
    string Noun,
    string Label,
    string InputType,
    int MaxLength,
    string SaveHandler,
    string DeleteHandler);
