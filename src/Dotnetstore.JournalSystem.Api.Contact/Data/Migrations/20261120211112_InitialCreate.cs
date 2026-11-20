using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dotnetstore.JournalSystem.Api.Contact.Data.Migrations;

/// <inheritdoc />
public partial class _20261120211112_InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "Contact");

        migrationBuilder.CreateTable(
            name: "Address",
            schema: "Contact",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AddressTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CO = table.Column<string>(type: "nvarchar(476)", maxLength: 476, nullable: true),
                Street1 = table.Column<string>(type: "nvarchar(876)", maxLength: 876, nullable: false),
                Street2 = table.Column<string>(type: "nvarchar(876)", maxLength: 876, nullable: true),
                PostalCode = table.Column<string>(type: "nvarchar(156)", maxLength: 156, nullable: false),
                City = table.Column<string>(type: "nvarchar(476)", maxLength: 476, nullable: false),
                State = table.Column<string>(type: "nvarchar(476)", maxLength: 476, nullable: false),
                Country = table.Column<string>(type: "nvarchar(476)", maxLength: 476, nullable: false),
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
                table.PrimaryKey("PK_Address", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AddressType",
            schema: "Contact",
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
                table.PrimaryKey("PK_AddressType", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "EmailAddress",
            schema: "Contact",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EmailAddressTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Address = table.Column<string>(type: "nvarchar(1092)", maxLength: 1092, nullable: false),
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
                table.PrimaryKey("PK_EmailAddress", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "EmailAddressType",
            schema: "Contact",
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
                table.PrimaryKey("PK_EmailAddressType", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PhoneNumber",
            schema: "Contact",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PhoneNumberTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Number = table.Column<string>(type: "nvarchar(196)", maxLength: 196, nullable: false),
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
                table.PrimaryKey("PK_PhoneNumber", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "PhoneNumberType",
            schema: "Contact",
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
                table.PrimaryKey("PK_PhoneNumberType", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Address",
            schema: "Contact");

        migrationBuilder.DropTable(
            name: "AddressType",
            schema: "Contact");

        migrationBuilder.DropTable(
            name: "EmailAddress",
            schema: "Contact");

        migrationBuilder.DropTable(
            name: "EmailAddressType",
            schema: "Contact");

        migrationBuilder.DropTable(
            name: "PhoneNumber",
            schema: "Contact");

        migrationBuilder.DropTable(
            name: "PhoneNumberType",
            schema: "Contact");
    }
}
