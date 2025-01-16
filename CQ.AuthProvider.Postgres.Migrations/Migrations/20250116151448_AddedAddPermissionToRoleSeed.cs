﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CQ.AuthProvider.Postgres.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddedAddPermissionToRoleSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppId", "Description", "IsPublic", "Key", "Name", "TenantId" },
                values: new object[] { new Guid("c402e13f-40c4-4b97-b004-d5e616c3f82d"), new Guid("f4ad89eb-6a0b-427a-8aef-b6bc736884dc"), "Can add permissions to role", true, "addpermission-role", "Can add permissions to role", new Guid("882a262c-e1a7-411d-a26e-40c61f3b810c") });

            migrationBuilder.InsertData(
                table: "RolesPermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[] { new Guid("c402e13f-40c4-4b97-b004-d5e616c3f82d"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("c402e13f-40c4-4b97-b004-d5e616c3f82d"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c402e13f-40c4-4b97-b004-d5e616c3f82d"));
        }
    }
}
