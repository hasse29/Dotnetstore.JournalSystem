namespace Dotnetstore.JournalSystem.Shared.Requests.Organization;

public sealed record ApproveAccessRequestRequest(Guid OwnCompanyId, Guid RoleId);
