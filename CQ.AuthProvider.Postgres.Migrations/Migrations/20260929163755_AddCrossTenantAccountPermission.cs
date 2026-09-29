using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CQ.AuthProvider.Postgres.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddCrossTenantAccountPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppId", "Description", "IsPublic", "Key", "Name", "TenantId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000010"), new Guid("f4ad89eb-6a0b-427a-8aef-b6bc736884dc"), "Can read accounts and apps of every tenant, not only of its own", false, "getallcrosstenant-account", "Can read accounts of all tenants", new Guid("882a262c-e1a7-411d-a26e-40c61f3b810c") });

            migrationBuilder.InsertData(
                table: "RolesPermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("27c1378d-39df-4a57-b025-fc96963955a6"), new Guid("780a89b1-9fd3-4cf6-b802-2882ebb3db92") },
                    { new Guid("6323b5da-b78c-4984-a56e-8206775d3e91"), new Guid("780a89b1-9fd3-4cf6-b802-2882ebb3db92") },
                    { new Guid("00000000-0000-0000-0000-000000000010"), new Guid("780a89b1-9fd3-4cf6-b802-2882ebb3db92") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000010"), new Guid("780a89b1-9fd3-4cf6-b802-2882ebb3db92") });

            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("27c1378d-39df-4a57-b025-fc96963955a6"), new Guid("780a89b1-9fd3-4cf6-b802-2882ebb3db92") });

            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("6323b5da-b78c-4984-a56e-8206775d3e91"), new Guid("780a89b1-9fd3-4cf6-b802-2882ebb3db92") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000010"));
        }
    }
}
