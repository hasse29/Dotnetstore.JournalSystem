using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetstore.JournalSystem.Api.Organization.Data.Migrations;

/// <inheritdoc />
public partial class AddAccessRequests : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AccessRequest",
            schema: "Organization",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                DecidedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DecisionNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AccessRequest", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AccessRequest_ExternalId",
            schema: "Organization",
            table: "AccessRequest",
            column: "ExternalId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AccessRequest_Status",
            schema: "Organization",
            table: "AccessRequest",
            column: "Status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AccessRequest",
            schema: "Organization");
    }
}
