namespace Dotnetstore.JournalSystem.Shared.Contracts.Events;

/// <summary>An own company no longer exists. Modules holding data for it remove that data.</summary>
public sealed record OwnCompanyDeleted(Guid OwnCompanyId);
