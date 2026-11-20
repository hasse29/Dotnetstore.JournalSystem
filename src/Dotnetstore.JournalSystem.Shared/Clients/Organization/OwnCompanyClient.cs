using Dotnetstore.JournalSystem.Shared.Constants;
using Dotnetstore.JournalSystem.Shared.Requests.Organization;
using Dotnetstore.JournalSystem.Shared.Responses.Organization;

namespace Dotnetstore.JournalSystem.Shared.Clients.Organization;

public interface IOwnCompanyClient : ICrudClient<EditOwnCompanyRequest, EditOwnCompanyRequest, OwnCompanyResponse>;

internal sealed class OwnCompanyClient(HttpClient http)
    : CrudClientBase<EditOwnCompanyRequest, EditOwnCompanyRequest, OwnCompanyResponse>(
        http,
        ApiEndpoints.Organization.OwnCompanies.GetAll,
        ApiEndpoints.Organization.OwnCompanies.GetById,
        ApiEndpoints.Organization.OwnCompanies.Create,
        ApiEndpoints.Organization.OwnCompanies.Update,
        ApiEndpoints.Organization.OwnCompanies.Delete),
      IOwnCompanyClient;
