using System.Net;
using Dotnetstore.JournalSystem.Shared.Clients;
using Dotnetstore.JournalSystem.Shared.Clients.Contact;
using Dotnetstore.JournalSystem.Shared.Clients.Organization;
using Dotnetstore.JournalSystem.Shared.Requests.Contact;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Contact;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.FrontEnd.WebUi.Tests.Pages.Companies;

internal class FakeCrudClient<TCreate, TUpdate, TResponse> : ICrudClient<TCreate, TUpdate, TResponse>
    where TCreate : class
    where TUpdate : class
    where TResponse : class
{
    public List<TResponse> Items { get; } = [];

    /// <summary>When true every call fails as it does when the API cannot be reached.</summary>
    public bool Unreachable { get; set; }

    public TCreate? Created { get; private set; }

    public (Guid Id, TUpdate Request)? Updated { get; private set; }

    public Guid? DeletedId { get; private set; }

    public Task<ApiResult<IReadOnlyList<TResponse>>> GetAllAsync(CancellationToken cancellationToken = default) => ListAsync(Items);

    public Task<ApiResult<TResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public Task<ApiResult<TResponse>> CreateAsync(TCreate request, CancellationToken cancellationToken = default)
    {
        Created = request;
        return Task.FromResult(ApiResult.Create<TResponse>(HttpStatusCode.Created, null));
    }

    public Task<ApiResult<TResponse>> UpdateAsync(Guid id, TUpdate request, CancellationToken cancellationToken = default)
    {
        Updated = (id, request);
        return Task.FromResult(ApiResult.Create<TResponse>(HttpStatusCode.OK, null));
    }

    public Task<ApiResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeletedId = id;
        return Task.FromResult(ApiResult.Create(HttpStatusCode.NoContent));
    }

    protected Task<ApiResult<IReadOnlyList<TResponse>>> ListAsync(IEnumerable<TResponse> items) =>
        Unreachable
            ? throw new HttpRequestException("down")
            : Task.FromResult(ApiResult.Create<IReadOnlyList<TResponse>>(HttpStatusCode.OK, items.ToList()));
}

internal sealed class FakeAddressClient : FakeCrudClient<EditAddressRequest, EditAddressRequest, AddressResponse>, IAddressClient
{
    public Task<ApiResult<IReadOnlyList<AddressResponse>>> GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default) =>
        ListAsync(Items.Where(x => x.ObjectId == objectId));
}

internal sealed class FakeEmailAddressClient : FakeCrudClient<EditEmailAddressRequest, EditEmailAddressRequest, EmailAddressResponse>, IEmailAddressClient
{
    public Task<ApiResult<IReadOnlyList<EmailAddressResponse>>> GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default) =>
        ListAsync(Items.Where(x => x.ObjectId == objectId));
}

internal sealed class FakePhoneNumberClient : FakeCrudClient<EditPhoneNumberRequest, EditPhoneNumberRequest, PhoneNumberResponse>, IPhoneNumberClient
{
    public Task<ApiResult<IReadOnlyList<PhoneNumberResponse>>> GetByObjectIdAsync(Guid objectId, CancellationToken cancellationToken = default) =>
        ListAsync(Items.Where(x => x.ObjectId == objectId));
}

internal sealed class FakeAddressTypeClient : FakeCrudClient<EditAddressTypeRequest, EditAddressTypeRequest, AddressTypeResponse>, IAddressTypeClient;

internal sealed class FakeEmailAddressTypeClient : FakeCrudClient<EditEmailAddressTypeRequest, EditEmailAddressTypeRequest, EmailAddressTypeResponse>, IEmailAddressTypeClient;

internal sealed class FakePhoneNumberTypeClient : FakeCrudClient<EditPhoneNumberTypeRequest, EditPhoneNumberTypeRequest, PhoneNumberTypeResponse>, IPhoneNumberTypeClient;

internal sealed class FakeUserClient : FakeCrudClient<CreateUserRequest, EditUserRequest, UserResponse>, IUserClient;

internal sealed class FakeRoleClient : FakeCrudClient<EditRoleRequest, EditRoleRequest, RoleResponse>, IRoleClient;
