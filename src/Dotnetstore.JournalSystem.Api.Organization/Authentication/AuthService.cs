using System.Security.Claims;
using Ardalis.Result;
using Dotnetstore.JournalSystem.Api.Organization.Services;
using Dotnetstore.JournalSystem.Api.Organization.Users;
using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;
using Dotnetstore.Security.Handlers;
using Microsoft.Extensions.Logging;

namespace Dotnetstore.JournalSystem.Api.Organization.Authentication;

internal sealed class AuthService(
    IOrganizationUnitOfWork organizationUnitOfWork,
    IHashHandler hashHandler,
    ITokenHandler tokenHandler,
    TimeProvider timeProvider,
    ILogger<AuthService> logger) : IAuthService
{
    internal const int MaxFailedAttempts = 5;
    internal static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    async ValueTask<Result<LoginResponse>> IAuthService.LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await organizationUnitOfWork
            .Users
            .GetByUsernameAsync(request.Username, cancellationToken)
            .ConfigureAwait(false);

        if (user is null || user.AuthProvider != AuthProvider.Local || !user.AccountIsApproved)
        {
            // Hash anyway so response time does not reveal whether the account exists.
            hashHandler.Hash(request.Password);
            return Result<LoginResponse>.Unauthorized();
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (user.IsLockedOut)
        {
            if (user.LockoutEnd is { } end && end > now)
            {
                logger.LockedOutLoginRejected(user.Id);
                return Result<LoginResponse>.Unauthorized();
            }

            await organizationUnitOfWork
                .Users
                .ClearLockoutAsync(user.Id, cancellationToken)
                .ConfigureAwait(false);
        }

        if (!hashHandler.Verify(request.Password, user.PasswordHash))
        {
            await organizationUnitOfWork
                .Users
                .RegisterFailedLoginAsync(user.Id, MaxFailedAttempts, now.Add(LockoutDuration), cancellationToken)
                .ConfigureAwait(false);

            logger.LoginFailed(user.Id);
            return Result<LoginResponse>.Unauthorized();
        }

        var newHash = hashHandler.NeedsRehash(user.PasswordHash) ? hashHandler.Hash(request.Password) : null;
        await organizationUnitOfWork
            .Users
            .RegisterSuccessfulLoginAsync(user.Id, now, newHash, cancellationToken)
            .ConfigureAwait(false);

        var role = await organizationUnitOfWork
            .Roles
            .GetByIdAsync(user.RoleId, cancellationToken)
            .ConfigureAwait(false);

        var claims = new List<Claim>
        {
            new(AuthClaimTypes.UserId, user.Id.ToString()),
            new(AuthClaimTypes.OwnCompanyId, user.OwnCompanyId.ToString())
        };
        if (role is not null)
        {
            claims.Add(new Claim(AuthClaimTypes.Role, role.Name));
        }

        var token = tokenHandler.CreateAccessToken(user.Id.ToString(), claims);

        logger.LoginSucceeded(user.Id);
        return Result<LoginResponse>.Success(new LoginResponse(token.Value, token.ExpiresAt));
    }
}
