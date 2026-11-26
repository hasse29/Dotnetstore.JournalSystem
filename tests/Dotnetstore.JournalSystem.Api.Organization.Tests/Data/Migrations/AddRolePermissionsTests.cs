using Dotnetstore.JournalSystem.Api.Organization.Data;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Fixtures;
using Dotnetstore.JournalSystem.Api.Organization.Tests.Roles;
using Dotnetstore.JournalSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Shouldly;

namespace Dotnetstore.JournalSystem.Api.Organization.Tests.Data.Migrations;

public sealed class AddRolePermissionsTests(SqlServerFixture fixture)
{
    private const string Before = "20261125085829_AddOwnCompanyImages";

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact]
    public async Task ExistingRolesKeepTheViewingTheyHadAndTheAdministratorGetsNoRows()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrganizationDataContext>()
            .UseSqlServer(fixture.CreateDatabaseConnectionString(), OrganizationDataContext.ConfigureSqlServer)
            .Options;
        await using var context = new OrganizationDataContext(options);
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync(Before, Token);
        var nurse = Guid.NewGuid();
        var administrator = Guid.NewGuid();
        context.Roles.AddRange(
            RoleTestData.Create(nurse, "Nurse"),
            RoleTestData.Create(administrator, RoleNames.Administrator, isSystem: true));
        await context.SaveChangesAsync(Token);

        try
        {
            // Act
            await migrator.MigrateAsync(cancellationToken: Token);

            // Assert
            var rows = await context.RolePermissions.AsNoTracking().ToListAsync(Token);
            rows.Where(x => x.RoleId == nurse).Select(x => x.Permission).ShouldBe(
                [
                    Permissions.OwnCompanies.View,
                    Permissions.Roles.View,
                    Permissions.WorkGroups.View,
                    Permissions.ContactDetails.View,
                    Permissions.ContactTypes.View
                ],
                ignoreOrder: true);
            rows.ShouldNotContain(x => x.RoleId == administrator);
        }
        finally
        {
            await context.Database.EnsureDeletedAsync(Token);
        }
    }
}
