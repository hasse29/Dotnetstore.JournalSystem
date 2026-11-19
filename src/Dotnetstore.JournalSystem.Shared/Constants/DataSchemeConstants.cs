namespace Dotnetstore.JournalSystem.Shared.Constants;

public static class DataSchemeConstants
{
    public const string SchemeName = "Organization";
    
    public const int OwnCompanyNameMaxLength = 100;
    public const int OwnCompanyDescriptionMaxLength = 500;
    public const int OwnCompanyCorporateNumberMaxLength = 50;
    public const int OwnCompanyTaxIdMaxLength = 20;
    
    public const int RoleNameMaxLength = 100;
    public const int RoleDescriptionMaxLength = 500;

    public const int WorkGroupNameMaxLength = 100;
    public const int WorkGroupDescriptionMaxLength = 500;

    public const string ContactSchemeName = "Contact";

    public const int AddressTypeNameMaxLength = 100;
    public const int AddressTypeDescriptionMaxLength = 500;
    public const int PhoneNumberTypeNameMaxLength = 100;
    public const int PhoneNumberTypeDescriptionMaxLength = 500;

    public const int EmailAddressTypeNameMaxLength = 100;
    public const int EmailAddressTypeDescriptionMaxLength = 500;

    public const int EmailAddressMaxLength = 254;

    public const int AddressCOMaxLength = 100;
    public const int AddressStreetMaxLength = 200;
    public const int AddressPostalCodeMaxLength = 20;
    public const int AddressCityMaxLength = 100;
    public const int AddressStateMaxLength = 100;
    public const int AddressCountryMaxLength = 100;
}