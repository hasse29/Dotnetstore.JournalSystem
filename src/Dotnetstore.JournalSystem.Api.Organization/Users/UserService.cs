using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Dotnetstore.Security.Handlers;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal sealed class UserService(
    IOrganizationUnitOfWork organizationUnitOfWork,
    IHashHandler hashHandler,
    IEntraUserProvisioner entraProvisioner,
    IPersonalIdentityNumberProtector identityNumbers) : IUserService
{
    async Task<IEnumerable<UserResponse>> IUserService.GetAllAsync(CancellationToken cancellationToken)
    {
        var items = await organizationUnitOfWork
            .Users
            .GetAllAsync(cancellationToken)
            .ConfigureAwait(false);

        return items.Select(x => x.ToUserResponse(identityNumbers.Mask(x.SocialSecurityNumber, x.Id)));
    }

    async ValueTask<Result<UserResponse>> IUserService.GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await organizationUnitOfWork
            .Users
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return user is not null ?
            Result<UserResponse>.Success(user.ToUserResponse(identityNumbers.Mask(user.SocialSecurityNumber, user.Id))) :
            Result<UserResponse>.NotFound();
    }

    async ValueTask<Result<UserResponse>> IUserService.CreateAsync(CreateUserRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        var references = await ValidateReferencesAsync(request.OwnCompanyId, request.RoleId, cancellationToken)
            .ConfigureAwait(false);
        if (references.Count > 0)
        {
            return Result<UserResponse>.Invalid(references);
        }

        var existing = await organizationUnitOfWork
            .Users
            .GetByUsernameAsync(request.Username, cancellationToken)
            .ConfigureAwait(false);
        if (existing is not null)
        {
            return Result<UserResponse>.Conflict("A user with that username already exists.");
        }

        var passwordHash = string.Empty;
        string? externalId = null;
        string? temporaryPassword = null;

        if (request.AuthProvider == AuthProvider.Local)
        {
            passwordHash = hashHandler.Hash(request.Password!);
        }
        else
        {
            var created = await entraProvisioner
                .CreateAsync(new EntraUserDetails(request.FirstName, request.LastName, request.Username), cancellationToken)
                .ConfigureAwait(false);
            if (!created.IsSuccess)
            {
                return ToFailure(created);
            }

            externalId = created.Value.ExternalId;
            temporaryPassword = created.Value.TemporaryPassword;
        }

        var user = UserBuilder
            .Create()
            .WithCreateValues(request, passwordHash, externalId, userId)
            .SetPersonalIdentityNumber(request.SocialSecurityNumber, identityNumbers)
            .Build();

        await organizationUnitOfWork
            .Users
            .AddAsync(user, cancellationToken)
            .ConfigureAwait(false);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        if (result > 0)
        {
            return Result<UserResponse>.Success(user.ToUserResponse(identityNumbers.Mask(user.SocialSecurityNumber, user.Id)) with { TemporaryPassword = temporaryPassword });
        }

        if (externalId is not null)
        {
            await entraProvisioner
                .DeleteAsync(externalId, cancellationToken)
                .ConfigureAwait(false);
        }

        return Result<UserResponse>.Error("Failed to create the user.");
    }

    async ValueTask<Result<UserResponse>> IUserService.UpdateAsync(Guid id, EditUserRequest request, Guid? userId, CancellationToken cancellationToken)
    {
        var existing = await organizationUnitOfWork
            .Users
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result<UserResponse>.NotFound();
        }

        var references = await ValidateReferencesAsync(request.OwnCompanyId, request.RoleId, cancellationToken)
            .ConfigureAwait(false);
        if (references.Count > 0)
        {
            return Result<UserResponse>.Invalid(references);
        }

        if (existing.AuthProvider == AuthProvider.Entra && existing.ExternalId is not null)
        {
            var updated = await entraProvisioner
                .UpdateAsync(
                    existing.ExternalId,
                    new EntraUserDetails(request.FirstName, request.LastName, existing.Username),
                    cancellationToken)
                .ConfigureAwait(false);
            if (!updated.IsSuccess)
            {
                return ToFailure(updated);
            }
        }

        var updateBuilder = UserBuilder
            .Update()
            .WithUpdateValues(existing, request, userId);

        if (request.SocialSecurityNumber is { } personalIdentityNumber)
        {
            updateBuilder.SetPersonalIdentityNumber(personalIdentityNumber, identityNumbers);
        }

        var userToUpdate = updateBuilder.Build();

        organizationUnitOfWork
            .Users
            .Detach(existing);

        organizationUnitOfWork
            .Users
            .Update(userToUpdate);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return result > 0 ?
            Result<UserResponse>.Success(userToUpdate.ToUserResponse(identityNumbers.Mask(userToUpdate.SocialSecurityNumber, userToUpdate.Id))) :
            Result<UserResponse>.Error("Failed to update the user.");
    }

    async ValueTask<Result<MyProfileResponse>> IUserService.GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await organizationUnitOfWork
            .Users
            .GetByIdAsync(userId, cancellationToken)
            .ConfigureAwait(false);
        if (user is null)
        {
            return Result<MyProfileResponse>.NotFound();
        }

        var company = await organizationUnitOfWork
            .OwnCompanies
            .GetByIdAsync(user.OwnCompanyId, cancellationToken)
            .ConfigureAwait(false);
        var role = await organizationUnitOfWork
            .Roles
            .GetByIdAsync(user.RoleId, cancellationToken)
            .ConfigureAwait(false);

        return Result<MyProfileResponse>.Success(user.ToMyProfileResponse(
            company?.Name ?? string.Empty,
            role?.Name ?? string.Empty,
            identityNumbers.Mask(user.SocialSecurityNumber, user.Id)));
    }

    async ValueTask<Result> IUserService.DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existing = await organizationUnitOfWork
            .Users
            .GetByIdAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return Result.NotFound();
        }

        if (existing.IsSystem)
        {
            return Result.Conflict("System users cannot be deleted.");
        }

        if (existing.AuthProvider == AuthProvider.Entra && existing.ExternalId is not null)
        {
            var deleted = await entraProvisioner
                .DeleteAsync(existing.ExternalId, cancellationToken)
                .ConfigureAwait(false);
            if (!deleted.IsSuccess)
            {
                return deleted;
            }
        }

        await organizationUnitOfWork
            .Users
            .DeleteAsync(id, cancellationToken)
            .ConfigureAwait(false);

        var result = await organizationUnitOfWork
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        if (result > 0 && existing.ExternalId is not null)
        {
            // Without this the person would stay "approved" forever and could never apply again.
            await organizationUnitOfWork
                .AccessRequests
                .DeleteByExternalIdAsync(existing.ExternalId, cancellationToken)
                .ConfigureAwait(false);
        }

        return result > 0 ?
            Result.Success() :
            Result.Error("Failed to delete the user.");
    }

    private static Result<UserResponse> ToFailure(Ardalis.Result.IResult failed)
    {
        return failed.Status switch
        {
            ResultStatus.Forbidden => Result<UserResponse>.Forbidden(),
            ResultStatus.Unavailable => Result<UserResponse>.Unavailable(failed.Errors.ToArray()),
            ResultStatus.Conflict => Result<UserResponse>.Conflict(failed.Errors.ToArray()),
            ResultStatus.NotFound => Result<UserResponse>.NotFound(),
            _ => Result<UserResponse>.Error(new ErrorList(failed.Errors))
        };
    }

    private async Task<List<ValidationError>> ValidateReferencesAsync(
        Guid ownCompanyId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var errors = new List<ValidationError>();

        var ownCompany = await organizationUnitOfWork
            .OwnCompanies
            .GetByIdAsync(ownCompanyId, cancellationToken)
            .ConfigureAwait(false);
        if (ownCompany is null)
        {
            errors.Add(new ValidationError(nameof(CreateUserRequest.OwnCompanyId), "The own company does not exist."));
        }

        var role = await organizationUnitOfWork
            .Roles
            .GetByIdAsync(roleId, cancellationToken)
            .ConfigureAwait(false);
        if (role is null)
        {
            errors.Add(new ValidationError(nameof(CreateUserRequest.RoleId), "The role does not exist."));
        }

        return errors;
    }
}
