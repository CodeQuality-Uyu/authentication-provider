using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CQ.AuthProvider.DataAccess.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class MakeSeedRolesPrivate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("01e55142-6b8c-4e7e-9d71-1e459d07796d"),
                column: "IsPublic",
                value: false);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5"),
                column: "IsPublic",
                value: false);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("780a89b1-9fd3-4cf6-b802-2882ebb3db92"),
                column: "IsPublic",
                value: false);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e"),
                column: "IsPublic",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("01e55142-6b8c-4e7e-9d71-1e459d07796d"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("780a89b1-9fd3-4cf6-b802-2882ebb3db92"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e"),
                column: "IsPublic",
                value: true);
        }
    }
}
