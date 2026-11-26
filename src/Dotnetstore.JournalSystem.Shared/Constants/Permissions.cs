namespace Dotnetstore.JournalSystem.Shared.Constants;

/// <summary>One kind of thing a role can be given rights to. A null right means the area does not have it.</summary>
public sealed record PermissionArea(string Label, string View, string? Create, string? Edit, string? Delete)
{
    public IEnumerable<string> All => new[] { View, Create, Edit, Delete }.OfType<string>();
}

/// <summary>
/// The rights a role can hold. The values are stored in the database and used as authorization policy names, so a
/// value must not change without a data migration. The Administrator role has all of them without holding any.
/// </summary>
public static class Permissions
{
    public static class OwnCompanies
    {
        public const string View = "own-companies:view";
        public const string Create = "own-companies:create";

        /// <summary>Includes the logo and photos.</summary>
        public const string Edit = "own-companies:edit";

        public const string Delete = "own-companies:delete";
    }

    public static class Users
    {
        public const string View = "users:view";
        public const string Create = "users:create";
        public const string Edit = "users:edit";
        public const string Delete = "users:delete";
    }

    /// <summary>Changing roles and their rights is for administrators only, so there is just the one right.</summary>
    public static class Roles
    {
        public const string View = "roles:view";
    }

    public static class WorkGroups
    {
        public const string View = "work-groups:view";
        public const string Create = "work-groups:create";
        public const string Edit = "work-groups:edit";
        public const string Delete = "work-groups:delete";
    }

    public static class AccessRequests
    {
        public const string View = "access-requests:view";

        /// <summary>Approve, reject and reopen.</summary>
        public const string Edit = "access-requests:edit";
    }

    public static class ContactDetails
    {
        public const string View = "contact-details:view";
        public const string Create = "contact-details:create";
        public const string Edit = "contact-details:edit";
        public const string Delete = "contact-details:delete";
    }

    public static class ContactTypes
    {
        public const string View = "contact-types:view";
        public const string Create = "contact-types:create";
        public const string Edit = "contact-types:edit";
        public const string Delete = "contact-types:delete";
    }

    public static IReadOnlyList<PermissionArea> Areas { get; } =
    [
        new("Own companies", OwnCompanies.View, OwnCompanies.Create, OwnCompanies.Edit, OwnCompanies.Delete),
        new("Users", Users.View, Users.Create, Users.Edit, Users.Delete),
        new("Roles", Roles.View, null, null, null),
        new("Work groups", WorkGroups.View, WorkGroups.Create, WorkGroups.Edit, WorkGroups.Delete),
        new("Access requests", AccessRequests.View, null, AccessRequests.Edit, null),
        new("Contact details", ContactDetails.View, ContactDetails.Create, ContactDetails.Edit, ContactDetails.Delete),
        new("Contact types", ContactTypes.View, ContactTypes.Create, ContactTypes.Edit, ContactTypes.Delete)
    ];

    public static IReadOnlyList<string> All { get; } = [.. Areas.SelectMany(x => x.All)];

    /// <summary>The rights that are of no use without others, and which ones those are.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> Requires { get; } = BuildRequirements();

    /// <summary>The given rights together with everything they require, in the order of <see cref="All"/>.</summary>
    public static IReadOnlyList<string> WithRequired(IEnumerable<string> permissions)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        var result = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>(permissions);
        while (pending.TryPop(out var permission))
        {
            if (result.Add(permission) && Requires.TryGetValue(permission, out var required))
            {
                foreach (var item in required)
                {
                    pending.Push(item);
                }
            }
        }

        return [.. All.Where(result.Contains)];
    }

    private static Dictionary<string, IReadOnlyList<string>> BuildRequirements()
    {
        var requirements = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        void Add(string permission, params string[] required)
        {
            if (!requirements.TryGetValue(permission, out var list))
            {
                requirements[permission] = list = [];
            }

            list.AddRange(required);
        }

        foreach (var area in Areas)
        {
            foreach (var change in area.All.Skip(1))
            {
                Add(change, area.View);
            }
        }

        // Users are shown with their role's name, and are placed in a company.
        Add(Users.View, Roles.View);
        Add(Users.Create, OwnCompanies.View);
        Add(Users.Edit, OwnCompanies.View);
        // Approving a request means choosing a role and a company.
        Add(AccessRequests.Edit, Roles.View, OwnCompanies.View);
        Add(ContactDetails.View, ContactTypes.View);

        return requirements.ToDictionary(x => x.Key, x => (IReadOnlyList<string>)x.Value, StringComparer.Ordinal);
    }
}
