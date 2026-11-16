using Dotnetstore.BaseModel.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.JournalSystem.Api.Organization.OwnCompanies;

internal sealed class OwnCompanyConfiguration : CompanyConfiguration<OwnCompany>
{
    public override void Configure(EntityTypeBuilder<OwnCompany> builder)
    {
        base.Configure(builder);
        
        builder
            .ToTable("OwnCompanies", "Organization")
            .HasKey(x => x.Id);
    }
}