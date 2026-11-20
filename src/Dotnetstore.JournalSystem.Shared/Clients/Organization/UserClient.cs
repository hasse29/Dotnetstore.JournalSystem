using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Shared.Clients.Organization;

public interface IUserClient : ICrudClient<CreateUserRequest, EditUserRequest, UserResponse>;

internal sealed class UserClient(HttpClient http)
    : CrudClientBase<CreateUserRequest, EditUserRequest, UserResponse>(
        http,
        ApiEndpoints.Organization.Users.GetAll,
        ApiEndpoints.Organization.Users.GetById,
        ApiEndpoints.Organization.Users.Create,
        ApiEndpoints.Organization.Users.Update,
        ApiEndpoints.Organization.Users.Delete),
      IUserClient;
