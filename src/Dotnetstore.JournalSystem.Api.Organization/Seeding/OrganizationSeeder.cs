using System.Text.Json;
using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.Seeding;

internal static class OrganizationSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal static async Task<int> SeedAsync(
        OrganizationDataContext context,
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
        OrganizationDataContext context,
        string seedFilePath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedFilePath);

        if (!File.Exists(seedFilePath))
        {
            return 0;
        }

        var json = await File.ReadAllTextAsync(seedFilePath, cancellationToken).ConfigureAwait(false);
        var data = JsonSerializer.Deserialize<OrganizationSeedData>(json, JsonOptions);
        if (data is null)
        {
            return 0;
        }

        var added = 0;
        foreach (var request in data.OwnCompanies)
        {
            var exists = await context.OwnCompanies
                .AnyAsync(x => x.IsSystem && x.Name == request.Name, cancellationToken)
                .ConfigureAwait(false);
            if (exists)
            {
                continue;
            }

            var ownCompany = OwnCompanyBuilder
                .Create()
                .WithCreateValues(request, null)
                .SetIsSystem(true)
                .Build();

            await context.OwnCompanies.AddAsync(ownCompany, cancellationToken).ConfigureAwait(false);
            added++;
        }

        if (added > 0)
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return added;
    }
}
