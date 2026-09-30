using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CQ.AuthProvider.DataAccess.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiAppScopePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppId", "Description", "IsPublic", "Key", "Name", "TenantId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000006"), new Guid("f4ad89eb-6a0b-427a-8aef-b6bc736884dc"), "Can give explicit scope over a role to descendant apps", false, "addapps-role", "Can add apps to role", new Guid("882a262c-e1a7-411d-a26e-40c61f3b810c") },
                    { new Guid("00000000-0000-0000-0000-000000000007"), new Guid("f4ad89eb-6a0b-427a-8aef-b6bc736884dc"), "Can remove the explicit scope over a role from an app", false, "removeapp-role", "Can remove app of role", new Guid("882a262c-e1a7-411d-a26e-40c61f3b810c") },
                    { new Guid("00000000-0000-0000-0000-000000000008"), new Guid("f4ad89eb-6a0b-427a-8aef-b6bc736884dc"), "Can give explicit scope over a permission to descendant apps", false, "addapps-permission", "Can add apps to permission", new Guid("882a262c-e1a7-411d-a26e-40c61f3b810c") },
                    { new Guid("00000000-0000-0000-0000-000000000009"), new Guid("f4ad89eb-6a0b-427a-8aef-b6bc736884dc"), "Can remove the explicit scope over a permission from an app", false, "removeapp-permission", "Can remove app of permission", new Guid("882a262c-e1a7-411d-a26e-40c61f3b810c") }
                });

            migrationBuilder.InsertData(
                table: "RolesPermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000006"), new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5") },
                    { new Guid("00000000-0000-0000-0000-000000000007"), new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5") },
                    { new Guid("00000000-0000-0000-0000-000000000008"), new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5") },
                    { new Guid("00000000-0000-0000-0000-000000000009"), new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5") },
                    { new Guid("00000000-0000-0000-0000-000000000006"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") },
                    { new Guid("00000000-0000-0000-0000-000000000007"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") },
                    { new Guid("00000000-0000-0000-0000-000000000008"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") },
                    { new Guid("00000000-0000-0000-0000-000000000009"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000006"), new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5") });

            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000007"), new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5") });

            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000008"), new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5") });

            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000009"), new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5") });

            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000006"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") });

            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000007"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") });

            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000008"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") });

            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000009"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000009"));
        }
    }
}
