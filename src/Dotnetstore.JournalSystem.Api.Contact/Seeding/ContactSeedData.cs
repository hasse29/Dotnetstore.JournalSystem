namespace Dotnetstore.JournalSystem.Api.Contact.Seeding;

internal sealed class ContactSeedData
{
    public List<SeedType> AddressTypes { get; init; } = [];
    public List<SeedType> PhoneNumberTypes { get; init; } = [];
    public List<SeedType> EmailAddressTypes { get; init; } = [];
}

internal sealed record SeedType(string Name, string Description = "");
