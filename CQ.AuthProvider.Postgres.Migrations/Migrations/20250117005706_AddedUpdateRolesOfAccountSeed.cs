﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CQ.AuthProvider.Postgres.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddedUpdateRolesOfAccountSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppId", "Description", "IsPublic", "Key", "Name", "TenantId" },
                values: new object[] { new Guid("c0a55e4b-b24d-42a4-90e4-f828e2b8e098"), new Guid("f4ad89eb-6a0b-427a-8aef-b6bc736884dc"), "Update roles of account. Roles of tenant and of apps of user logged", true, "udpdateroles-account", "Update roles of account", new Guid("882a262c-e1a7-411d-a26e-40c61f3b810c") });

            migrationBuilder.InsertData(
                table: "RolesPermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { new Guid("c0a55e4b-b24d-42a4-90e4-f828e2b8e098"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("c0a55e4b-b24d-42a4-90e4-f828e2b8e098"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c0a55e4b-b24d-42a4-90e4-f828e2b8e098"));
        }
    }
}
