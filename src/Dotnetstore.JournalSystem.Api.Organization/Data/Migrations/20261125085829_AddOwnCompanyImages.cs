using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetstore.JournalSystem.Api.Organization.Data.Migrations;

/// <inheritdoc />
public partial class AddOwnCompanyImages : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OwnCompanyImage",
            schema: "Organization",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OwnCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsLogo = table.Column<bool>(type: "bit", nullable: false),
                FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                SizeBytes = table.Column<int>(type: "int", nullable: false),
                Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OwnCompanyImage", x => x.Id);
                table.ForeignKey(
                    name: "FK_OwnCompanyImage_OwnCompany_OwnCompanyId",
                    column: x => x.OwnCompanyId,
                    principalSchema: "Organization",
                    principalTable: "OwnCompany",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OwnCompanyImage_Logo",
            schema: "Organization",
            table: "OwnCompanyImage",
            column: "OwnCompanyId",
            unique: true,
            filter: "[IsLogo] = 1");

        migrationBuilder.CreateIndex(
            name: "IX_OwnCompanyImage_OwnCompanyId",
            schema: "Organization",
            table: "OwnCompanyImage",
            column: "OwnCompanyId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OwnCompanyImage",
            schema: "Organization");
    }
}
