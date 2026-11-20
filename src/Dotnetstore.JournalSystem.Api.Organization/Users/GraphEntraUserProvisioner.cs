using System.Security.Cryptography;
using Ardalis.Result;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ODataErrors;
using Dotnetstore.JournalSystem.Api.Organization.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;

namespace Dotnetstore.JournalSystem.Api.Organization.Users;

internal sealed class GraphEntraUserProvisioner(
    GraphServiceClient graph,
    ILogger<GraphEntraUserProvisioner> logger) : IEntraUserProvisioner
{
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%^&*-_=+";
    private const int TemporaryPasswordLength = 20;

    async ValueTask<Result<EntraUserCreated>> IEntraUserProvisioner.CreateAsync(
        EntraUserDetails details,
        CancellationToken cancellationToken)
    {
        var temporaryPassword = GenerateTemporaryPassword();

        var user = new Microsoft.Graph.Models.User
        {
            AccountEnabled = true,
            DisplayName = $"{details.FirstName} {details.LastName}",
            GivenName = details.FirstName,
            Surname = details.LastName,
            UserPrincipalName = details.Username,
            MailNickname = ToMailNickname(details.Username),
            PasswordProfile = new PasswordProfile
            {
                Password = temporaryPassword,
                ForceChangePasswordNextSignIn = true
            }
        };

        try
        {
            var created = await graph.Users
                .PostAsync(user, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(created?.Id))
            {
                return Result<EntraUserCreated>.Error("Entra ID did not return the id of the created user.");
            }

            logger.EntraUserChanged(created.Id, "created");
            return Result<EntraUserCreated>.Success(new EntraUserCreated(created.Id, temporaryPassword));
        }
        catch (ODataError ex)
        {
            return ToFailure<EntraUserCreated>(ex, "create");
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            return ToTokenFailure<EntraUserCreated>(ex);
        }
        catch (MsalException ex)
        {
            return ToTokenFailure<EntraUserCreated>(ex);
        }
    }

    async ValueTask<Result> IEntraUserProvisioner.UpdateAsync(
        string externalId,
        EntraUserDetails details,
        CancellationToken cancellationToken)
    {
        var patch = new Microsoft.Graph.Models.User
        {
            DisplayName = $"{details.FirstName} {details.LastName}",
            GivenName = details.FirstName,
            Surname = details.LastName
        };

        try
        {
            await graph.Users[externalId]
                .PatchAsync(patch, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            logger.EntraUserChanged(externalId, "updated");
            return Result.Success();
        }
        catch (ODataError ex)
        {
            return ToFailure(ex, "update");
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            return ToTokenFailure(ex);
        }
        catch (MsalException ex)
        {
            return ToTokenFailure(ex);
        }
    }

    async ValueTask<Result> IEntraUserProvisioner.DeleteAsync(string externalId, CancellationToken cancellationToken)
    {
        try
        {
            await graph.Users[externalId]
                .DeleteAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            logger.EntraUserChanged(externalId, "deleted");
            return Result.Success();
        }
        catch (ODataError ex) when (ex.ResponseStatusCode == 404)
        {
            return Result.Success();
        }
        catch (ODataError ex)
        {
            return ToFailure(ex, "delete");
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            return ToTokenFailure(ex);
        }
        catch (MsalException ex)
        {
            return ToTokenFailure(ex);
        }
    }

    internal static string GenerateTemporaryPassword()
    {
        const string all = Lower + Upper + Digits + Symbols;

        var chars = new char[TemporaryPasswordLength];
        chars[0] = Pick(Lower);
        chars[1] = Pick(Upper);
        chars[2] = Pick(Digits);
        chars[3] = Pick(Symbols);

        for (var i = 4; i < chars.Length; i++)
        {
            chars[i] = Pick(all);
        }

        RandomNumberGenerator.Shuffle<char>(chars);
        return new string(chars);
    }

    internal static string ToMailNickname(string username)
    {
        var local = username.Split('@')[0];
        var cleaned = new string(local.Where(char.IsAsciiLetterOrDigit).ToArray());
        return cleaned.Length > 0 ? cleaned : Guid.NewGuid().ToString("N")[..12];
    }

    private static char Pick(string alphabet)
    {
        return alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
    }

    private Result<T> ToFailure<T>(ODataError error, string operation)
    {
        logger.EntraOperationFailed(operation, error.ResponseStatusCode, error.Error?.Code);

        var message = error.Error?.Message ?? "Entra ID rejected the request.";
        return error.ResponseStatusCode switch
        {
            401 or 403 => Result<T>.Forbidden(),
            404 => Result<T>.NotFound(),
            409 => Result<T>.Conflict(message),
            _ => Result<T>.Error(message)
        };
    }

    private Result ToFailure(ODataError error, string operation)
    {
        var failure = ToFailure<object>(error, operation);
        return failure.Status switch
        {
            ResultStatus.Forbidden => Result.Forbidden(),
            ResultStatus.NotFound => Result.NotFound(),
            ResultStatus.Conflict => Result.Conflict(failure.Errors.ToArray()),
            _ => Result.Error(new ErrorList(failure.Errors))
        };
    }

    private Result<T> ToTokenFailure<T>(Exception exception)
    {
        logger.GraphTokenUnavailable(exception);
        return Result<T>.Forbidden();
    }

    private Result ToTokenFailure(Exception exception)
    {
        logger.GraphTokenUnavailable(exception);
        return Result.Forbidden();
    }
}
