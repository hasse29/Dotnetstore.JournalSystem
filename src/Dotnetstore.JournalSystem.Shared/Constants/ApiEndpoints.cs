namespace Dotnetstore.JournalSystem.Shared.Constants;

public static class ApiEndpoints
{
    private const string Base = "/api";
    
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
    }
}