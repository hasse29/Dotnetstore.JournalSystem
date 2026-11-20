using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetstore.JournalSystem.Api.Organization.Data.Migrations;

/// <inheritdoc />
public partial class _20261120211106_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Organization");

        migrationBuilder.CreateTable(
            name: "OwnCompany",
            schema: "Organization",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastUpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsSystem = table.Column<bool>(type: "bit", nullable: false),
                IsGdpr = table.Column<bool>(type: "bit", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                LogoUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                WebsiteUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                CorporateNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                TaxId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OwnCompany", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Role",
            schema: "Organization",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastUpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsSystem = table.Column<bool>(type: "bit", nullable: false),
                IsGdpr = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Role", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "User",
            schema: "Organization",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                OwnCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AuthProvider = table.Column<int>(type: "int", nullable: false),
                ExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastUpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsSystem = table.Column<bool>(type: "bit", nullable: false),
                IsGdpr = table.Column<bool>(type: "bit", nullable: false),
                LastName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                FirstName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                MiddleName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                SocialSecurityNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                IsMale = table.Column<bool>(type: "bit", nullable: false),
                Username = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                SecurityStamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                EmailAddressConfirmed = table.Column<bool>(type: "bit", nullable: false),
                EmailAddressConfirmationToken = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                AccountIsApproved = table.Column<bool>(type: "bit", nullable: false),
                IsLockedOut = table.Column<bool>(type: "bit", nullable: false),
                LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_User", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "WorkGroup",
            schema: "Organization",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                LastUpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                IsSystem = table.Column<bool>(type: "bit", nullable: false),
                IsGdpr = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WorkGroup", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OwnCompany_IsDeleted",
            schema: "Organization",
            table: "OwnCompany",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_User_ExternalId",
            schema: "Organization",
            table: "User",
            column: "ExternalId",
            unique: true,
            filter: "[ExternalId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_User_IsDeleted",
            schema: "Organization",
            table: "User",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_User_OwnCompanyId",
            schema: "Organization",
            table: "User",
            column: "OwnCompanyId");

        migrationBuilder.CreateIndex(
            name: "IX_User_RoleId",
            schema: "Organization",
            table: "User",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "IX_User_Username",
            schema: "Organization",
            table: "User",
            column: "Username",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OwnCompany",
            schema: "Organization");

        migrationBuilder.DropTable(
            name: "Role",
            schema: "Organization");

        migrationBuilder.DropTable(
            name: "User",
            schema: "Organization");

        migrationBuilder.DropTable(
            name: "WorkGroup",
            schema: "Organization");
    }
}
