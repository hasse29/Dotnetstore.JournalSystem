using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.Roles;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.Security.Handlers;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.Seeding;

public sealed class BootstrapAdminOptions
{
    public const string SectionName = "Bootstrap:Admin";

    public string? Username { get; set; }
    public string FirstName { get; set; } = "System";
    public string LastName { get; set; } = "Administrator";
    public string? Password { get; set; }
    public string? EntraObjectId { get; set; }
}

internal static class OrganizationBootstrapper
{
    internal static async Task<bool> BootstrapAdminAsync(
        OrganizationDataContext context,
        IHashHandler hashHandler,
        BootstrapAdminOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(hashHandler);

        if (options is null || string.IsNullOrWhiteSpace(options.Username))
        {
            return false;
        }

        var hasEntra = !string.IsNullOrWhiteSpace(options.EntraObjectId);
        var hasPassword = !string.IsNullOrWhiteSpace(options.Password);

        // The minimum length is deliberately not enforced here: this is an operator-supplied secret, not user input.
        if ((!hasEntra && !hasPassword) ||
            (hasPassword && options.Password!.Length > DataSchemeConstants.UserPasswordMaxLength))
        {
            return false;
        }

        var existing = await context.Users
            .FirstOrDefaultAsync(x => x.Username == options.Username, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            return await LinkExistingAsync(context, existing, options, cancellationToken).ConfigureAwait(false);
        }

        var ownCompany = await context.OwnCompanies
            .FirstOrDefaultAsync(x => x.IsSystem, cancellationToken)
            .ConfigureAwait(false);
        if (ownCompany is null)
        {
            return false;
        }

        var role = await context.Roles
            .FirstOrDefaultAsync(x => x.IsSystem && x.Name == RoleNames.Administrator, cancellationToken)
            .ConfigureAwait(false);
        if (role is null)
        {
            role = RoleBuilder
                .Create()
                .WithCreateValues(new EditRoleRequest(RoleNames.Administrator, "Full access to the system."), null)
                .SetIsSystem(true)
                .Build();
            await context.Roles.AddAsync(role, cancellationToken).ConfigureAwait(false);
        }

        var request = new CreateUserRequest(
            options.FirstName,
            null,
            options.LastName,
            options.Username,
            ownCompany.Id,
            role.Id,
            hasPassword ? AuthProvider.Local : AuthProvider.Entra,
            null);

        var user = UserBuilder
            .Create()
            .WithCreateValues(
                request,
                hasPassword ? hashHandler.Hash(options.Password!) : string.Empty,
                hasEntra ? options.EntraObjectId : null,
                null)
            .SetIsSystem(true)
            .Build();

        await context.Users.AddAsync(user, cancellationToken).ConfigureAwait(false);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    // Lets an operator attach an Entra identity to an administrator that already exists, so one person
    // has one account that works with both a password and a Microsoft sign-in.
    private static async Task<bool> LinkExistingAsync(
        OrganizationDataContext context,
        User existing,
        BootstrapAdminOptions options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.EntraObjectId) || existing.ExternalId is not null)
        {
            return false;
        }

        var taken = await context.Users
            .AnyAsync(x => x.ExternalId == options.EntraObjectId, cancellationToken)
            .ConfigureAwait(false);
        if (taken)
        {
            return false;
        }

        existing.ExternalId = options.EntraObjectId;
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
