using System.Text.Json;
using Dotnetstore.JournalSystem.Api.Contact.AddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.Data;
using Dotnetstore.JournalSystem.Api.Contact.EmailAddressTypes;
using Dotnetstore.JournalSystem.Api.Contact.PhoneNumberTypes;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Contact.Seeding;

internal static class ContactSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal static async Task<int> SeedAsync(
        ContactDataContext context,
        IEnumerable<string> seedFilePaths,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(seedFilePaths);

        var added = 0;
        foreach (var path in seedFilePaths)
        {
            added += await SeedFileAsync(context, path, cancellationToken).ConfigureAwait(false);
        }

        return added;
    }

    private static async Task<int> SeedFileAsync(
        ContactDataContext context,
        string seedFilePath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedFilePath);

        if (!File.Exists(seedFilePath))
        {
            return 0;
        }

        var json = await File.ReadAllTextAsync(seedFilePath, cancellationToken).ConfigureAwait(false);
        var data = JsonSerializer.Deserialize<ContactSeedData>(json, JsonOptions);
        if (data is null)
        {
            return 0;
        }

        var added = 0;

        foreach (var item in data.AddressTypes)
        {
            if (await context.AddressTypes.AnyAsync(x => x.IsSystem && x.Name == item.Name, cancellationToken).ConfigureAwait(false))
            {
                continue;
            }

            var entity = AddressTypeBuilder.Create()
                .WithCreateValues(new EditAddressTypeRequest(item.Name, item.Description), null)
                .SetIsSystem(true)
                .Build();
            await context.AddressTypes.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            added++;
        }

        foreach (var item in data.PhoneNumberTypes)
        {
            if (await context.PhoneNumberTypes.AnyAsync(x => x.IsSystem && x.Name == item.Name, cancellationToken).ConfigureAwait(false))
            {
                continue;
            }

            var entity = PhoneNumberTypeBuilder.Create()
                .WithCreateValues(new EditPhoneNumberTypeRequest(item.Name, item.Description), null)
                .SetIsSystem(true)
                .Build();
            await context.PhoneNumberTypes.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            added++;
        }

        foreach (var item in data.EmailAddressTypes)
        {
            if (await context.EmailAddressTypes.AnyAsync(x => x.IsSystem && x.Name == item.Name, cancellationToken).ConfigureAwait(false))
            {
                continue;
            }

            var entity = EmailAddressTypeBuilder.Create()
                .WithCreateValues(new EditEmailAddressTypeRequest(item.Name, item.Description), null)
                .SetIsSystem(true)
                .Build();
            await context.EmailAddressTypes.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            added++;
        }

        if (added > 0)
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return added;
    }
}
