using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.JournalSystem.Api.Organization.AccessRequests;

internal sealed class AccessRequestService(
    IOrganizationUnitOfWork organizationUnitOfWork,
    TimeProvider clock) : IAccessRequestService
{
    async ValueTask<Result<AccessRequestResponse>> IAccessRequestService.RequestAccessAsync(
        EntraIdentity identity,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(identity);

        var existingUser = await organizationUnitOfWork
            .Users
            .GetByExternalIdAsync(identity.ExternalId, cancellationToken)
            .ConfigureAwait(false);
        if (existingUser is not null)
        {
            return Result<AccessRequestResponse>.Success(ForExistingUser(existingUser));
        }

        var existing = await organizationUnitOfWork
            .AccessRequests
            .GetByExternalIdAsync(identity.ExternalId, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            return Result<AccessRequestResponse>.Success(existing.ToAccessRequestResponse());
        }

        var request = AccessRequest.Create(identity.ExternalId, identity.DisplayName, identity.Username, clock.GetUtcNow().UtcDateTime);

        await organizationUnitOfWork
            .AccessRequests
            .AddAsync(request, cancellationToken)
            .ConfigureAwait(false);

        try
        {
            await organizationUnitOfWork
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (DbUpdateException)
        {
            // Two sign-ins raced; the unique index kept one request, so return that one.
            organizationUnitOfWork.AccessRequests.Detach(request);
            var winner = await organizationUnitOfWork
                .AccessRequests
                .GetByExternalIdAsync(identity.ExternalId, cancellationToken)
                .ConfigureAwait(false);

            return winner is not null
                ? Result<AccessRequestResponse>.Success(winner.ToAccessRequestResponse())
                : Result<AccessRequestResponse>.Error("Failed to create the access request.");
        }

        return Result<AccessRequestResponse>.Success(request.ToAccessRequestResponse());
    }

    async ValueTask<Result<AccessRequestResponse>> IAccessRequestService.GetMineAsync(
        string externalId,
        CancellationToken cancellationToken)
    {
        var request = await organizationUnitOfWork
            .AccessRequests
            .GetByExternalIdAsync(externalId, cancellationToken)
            .ConfigureAwait(false);

        if (request is not null)
        {
            return Result<AccessRequestResponse>.Success(request.ToAccessRequestResponse());
        }

        // People added directly by an administrator never applied, but are approved all the same.
        var user = await organizationUnitOfWork
            .Users
            .GetByExternalIdAsync(externalId, cancellationToken)
            .ConfigureAwait(false);

        return user is not null
            ? Result<AccessRequestResponse>.Success(ForExistingUser(user))
            : Result<AccessRequestResponse>.NotFound();
    }

    async ValueTask<IReadOnlyList<AccessRequestResponse>> IAccessRequestService.GetAllAsync(
        AccessRequestStatus? status,
        CancellationToken cancellationToken)
    {
        var items = await organizationUnitOfWork
            .AccessRequests
            .GetByStatusAsync(status, cancellationToken)
            .ConfigureAwait(false);

        return [.. items.Select(x => x.ToAccessRequestResponse())];
    }

    async ValueTask<Result<AccessRequestResponse>> IAccessRequestService.ApproveAsync(
        Guid id,
        ApproveAccessRequestRequest request,
        Guid? decidedBy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var accessRequest = await organizationUnitOfWork
            .AccessRequests
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);
        if (accessRequest is null)
        {
            return Result<AccessRequestResponse>.NotFound();
        }

        if (accessRequest.Status != AccessRequestStatus.Pending)
        {
            return Result<AccessRequestResponse>.Conflict("Only a pending request can be approved.");
        }

        var references = await ValidateReferencesAsync(request, cancellationToken).ConfigureAwait(false);
        if (references.Count > 0)
        {
            return Result<AccessRequestResponse>.Invalid(references);
        }

        var usernameTaken = await organizationUnitOfWork
            .Users
            .GetByUsernameAsync(accessRequest.Username, cancellationToken)
            .ConfigureAwait(false);
        var externalIdTaken = await organizationUnitOfWork
            .Users
            .GetByExternalIdAsync(accessRequest.ExternalId, cancellationToken)
            .ConfigureAwait(false);
        if (usernameTaken is not null || externalIdTaken is not null)
        {
            return Result<AccessRequestResponse>.Conflict("A user with that username or Entra identity already exists.");
        }

        var (firstName, lastName) = SplitName(accessRequest.DisplayName, accessRequest.Username);
        var user = UserBuilder
            .Create()
            .WithCreateValues(
                new CreateUserRequest(firstName, null, lastName, accessRequest.Username, request.OwnCompanyId, request.RoleId, AuthProvider.Entra, null),
                string.Empty,
                accessRequest.ExternalId,
                decidedBy)
            .Build();

        await organizationUnitOfWork
            .Users
            .AddAsync(user, cancellationToken)
            .ConfigureAwait(false);

        accessRequest.TryApprove(decidedBy, clock.GetUtcNow().UtcDateTime);

        // The new user and the decision are saved together so an approval can never half-apply.
        var saved = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return saved > 0
            ? Result<AccessRequestResponse>.Success(accessRequest.ToAccessRequestResponse())
            : Result<AccessRequestResponse>.Error("Failed to approve the access request.");
    }

    async ValueTask<Result<AccessRequestResponse>> IAccessRequestService.RejectAsync(
        Guid id,
        RejectAccessRequestRequest request,
        Guid? decidedBy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await DecideAsync(
                id,
                accessRequest => accessRequest.TryReject(decidedBy, request.Note, clock.GetUtcNow().UtcDateTime),
                "Only a pending request can be rejected.",
                cancellationToken)
            .ConfigureAwait(false);
    }

    async ValueTask<Result<AccessRequestResponse>> IAccessRequestService.ReopenAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await DecideAsync(
                id,
                accessRequest => accessRequest.TryReopen(),
                "Only a rejected request can be reopened.",
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static AccessRequestResponse ForExistingUser(User user) => new(
        user.Id,
        $"{user.FirstName} {user.LastName}".Trim(),
        user.Username,
        AccessRequestStatus.Approved,
        user.CreatedDate,
        null,
        null);

    internal static (string FirstName, string LastName) SplitName(string displayName, string fallback)
    {
        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            parts = [fallback];
        }

        var last = parts.Length > 1 ? parts[^1] : parts[0];
        var first = parts.Length > 1 ? string.Join(' ', parts[..^1]) : parts[0];

        return (Truncate(first), Truncate(last));
    }

    private static string Truncate(string value) =>
        value.Length <= DataSchemeConstants.UserNameMaxLength ? value : value[..DataSchemeConstants.UserNameMaxLength];

    private async ValueTask<Result<AccessRequestResponse>> DecideAsync(
        Guid id,
        Func<AccessRequest, bool> apply,
        string conflictMessage,
        CancellationToken cancellationToken)
    {
        var accessRequest = await organizationUnitOfWork
            .AccessRequests
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);
        if (accessRequest is null)
        {
            return Result<AccessRequestResponse>.NotFound();
        }

        if (!apply(accessRequest))
        {
            return Result<AccessRequestResponse>.Conflict(conflictMessage);
        }

        var saved = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return saved > 0
            ? Result<AccessRequestResponse>.Success(accessRequest.ToAccessRequestResponse())
            : Result<AccessRequestResponse>.Error("Failed to update the access request.");
    }

    private async Task<List<ValidationError>> ValidateReferencesAsync(
        ApproveAccessRequestRequest request,
        CancellationToken cancellationToken)
    {
        var errors = new List<ValidationError>();

        var ownCompany = await organizationUnitOfWork
            .OwnCompanies
            .GetByIdAsync(request.OwnCompanyId, cancellationToken)
            .ConfigureAwait(false);
        if (ownCompany is null)
        {
            errors.Add(new ValidationError(nameof(ApproveAccessRequestRequest.OwnCompanyId), "The own company does not exist."));
        }

        var role = await organizationUnitOfWork
            .Roles
            .GetByIdAsync(request.RoleId, cancellationToken)
            .ConfigureAwait(false);
        if (role is null)
        {
            errors.Add(new ValidationError(nameof(ApproveAccessRequestRequest.RoleId), "The role does not exist."));
        }

        return errors;
    }
}
