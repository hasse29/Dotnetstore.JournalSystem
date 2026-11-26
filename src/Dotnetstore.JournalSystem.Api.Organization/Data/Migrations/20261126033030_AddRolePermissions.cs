using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetstore.JournalSystem.Api.Organization.Data.Migrations;

/// <inheritdoc />
public partial class AddRolePermissions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RolePermission",
            schema: "Organization",
            columns: table => new
            {
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Permission = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RolePermission", x => new { x.RoleId, x.Permission });
                table.ForeignKey(
                    name: "FK_RolePermission_Role_RoleId",
                    column: x => x.RoleId,
                    principalSchema: "Organization",
                    principalTable: "Role",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // Before rights existed every approved user could view these, so the roles that already exist keep that.
        // The values are written out because a migration must not change when the code does.
        migrationBuilder.Sql(
            """
            INSERT INTO [Organization].[RolePermission] ([RoleId], [Permission])
            SELECT [r].[Id], [p].[Permission]
            FROM [Organization].[Role] AS [r]
            CROSS JOIN (VALUES
                (N'own-companies:view'),
                (N'roles:view'),
                (N'work-groups:view'),
                (N'contact-details:view'),
                (N'contact-types:view')) AS [p] ([Permission])
            WHERE NOT ([r].[IsSystem] = 1 AND [r].[Name] = N'Administrator');
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RolePermission",
            schema: "Organization");
    }
}
