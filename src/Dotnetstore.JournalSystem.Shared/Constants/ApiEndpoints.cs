namespace Dotnetstore.JournalSystem.Shared.Constants;

public static class ApiEndpoints
{
    private const string Base = "/api";

    public static class Auth
    {
        public const string Login = $"{Base}/auth/login";
    }

    public static class Organization
    {
        private const string OrganizationBase = $"{Base}/organization";

        public static class OwnCompanies
        {
            private const string OwnCompanyBase = $"{OrganizationBase}/own-companies";

            public const string GetAll = OwnCompanyBase;
            public const string GetById = $"{OwnCompanyBase}/{{id:guid}}";
            public const string Create = OwnCompanyBase;
            public const string Update = $"{OwnCompanyBase}/{{id:guid}}";
            public const string Delete = $"{OwnCompanyBase}/{{id:guid}}";
        }

        public static class Roles
        {
            private const string RoleBase = $"{OrganizationBase}/roles";

            public const string GetAll = RoleBase;
            public const string GetById = $"{RoleBase}/{{id:guid}}";
            public const string Create = RoleBase;
            public const string Update = $"{RoleBase}/{{id:guid}}";
            public const string Delete = $"{RoleBase}/{{id:guid}}";
        }

        public static class Users
        {
            private const string UserBase = $"{OrganizationBase}/users";

            public const string GetAll = UserBase;
            public const string GetById = $"{UserBase}/{{id:guid}}";
            public const string Create = UserBase;
            public const string Update = $"{UserBase}/{{id:guid}}";
            public const string Delete = $"{UserBase}/{{id:guid}}";
        }

        public static class WorkGroups
        {
            private const string WorkGroupBase = $"{OrganizationBase}/work-groups";

            public const string GetAll = WorkGroupBase;
            public const string GetById = $"{WorkGroupBase}/{{id:guid}}";
            public const string Create = WorkGroupBase;
            public const string Update = $"{WorkGroupBase}/{{id:guid}}";
            public const string Delete = $"{WorkGroupBase}/{{id:guid}}";
        }
    }

    public static class Contact
    {
        private const string ContactBase = $"{Base}/contact";

        public static class AddressTypes
        {
            private const string AddressTypeBase = $"{ContactBase}/address-types";

            public const string GetAll = AddressTypeBase;
            public const string GetById = $"{AddressTypeBase}/{{id:guid}}";
            public const string Create = AddressTypeBase;
            public const string Update = $"{AddressTypeBase}/{{id:guid}}";
            public const string Delete = $"{AddressTypeBase}/{{id:guid}}";
        }

        public static class PhoneNumberTypes
        {
            private const string PhoneNumberTypeBase = $"{ContactBase}/phone-number-types";

            public const string GetAll = PhoneNumberTypeBase;
            public const string GetById = $"{PhoneNumberTypeBase}/{{id:guid}}";
            public const string Create = PhoneNumberTypeBase;
            public const string Update = $"{PhoneNumberTypeBase}/{{id:guid}}";
            public const string Delete = $"{PhoneNumberTypeBase}/{{id:guid}}";
        }

        public static class EmailAddressTypes
        {
            private const string EmailAddressTypeBase = $"{ContactBase}/email-address-types";

            public const string GetAll = EmailAddressTypeBase;
            public const string GetById = $"{EmailAddressTypeBase}/{{id:guid}}";
            public const string Create = EmailAddressTypeBase;
            public const string Update = $"{EmailAddressTypeBase}/{{id:guid}}";
            public const string Delete = $"{EmailAddressTypeBase}/{{id:guid}}";
        }

        public static class Addresses
        {
            private const string AddressBase = $"{ContactBase}/addresses";

            public const string GetAll = AddressBase;
            public const string GetById = $"{AddressBase}/{{id:guid}}";
            public const string Create = AddressBase;
            public const string Update = $"{AddressBase}/{{id:guid}}";
            public const string Delete = $"{AddressBase}/{{id:guid}}";
        }

        public static class EmailAddresses
        {
            private const string EmailAddressBase = $"{ContactBase}/email-addresses";

            public const string GetAll = EmailAddressBase;
            public const string GetById = $"{EmailAddressBase}/{{id:guid}}";
            public const string Create = EmailAddressBase;
            public const string Update = $"{EmailAddressBase}/{{id:guid}}";
            public const string Delete = $"{EmailAddressBase}/{{id:guid}}";
        }

        public static class PhoneNumbers
        {
            private const string PhoneNumberBase = $"{ContactBase}/phone-numbers";

            public const string GetAll = PhoneNumberBase;
            public const string GetById = $"{PhoneNumberBase}/{{id:guid}}";
            public const string Create = PhoneNumberBase;
            public const string Update = $"{PhoneNumberBase}/{{id:guid}}";
            public const string Delete = $"{PhoneNumberBase}/{{id:guid}}";
        }
    }
}