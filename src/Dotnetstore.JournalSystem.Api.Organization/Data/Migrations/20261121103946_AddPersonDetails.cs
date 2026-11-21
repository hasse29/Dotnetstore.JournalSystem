using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetstore.JournalSystem.Api.Organization.Data.Migrations;

/// <inheritdoc />
public partial class AddPersonDetails : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "SocialSecurityNumber",
            schema: "Organization",
            table: "User",
            type: "nvarchar(124)",
            maxLength: 124,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50,
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "SocialSecurityNumber",
            schema: "Organization",
            table: "User",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(124)",
            oldMaxLength: 124,
            oldNullable: true);
    }
}
