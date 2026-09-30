using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CQ.AuthProvider.DataAccess.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAppBackgroundAndTenantLogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // updatecolors-app: el endpoint que lo pedía (PATCH /apps/{id}/colors) se eliminó. Se
            // borran TODAS sus referencias, no sólo las dos sembradas: roles creados por usuarios o
            // suscripciones que lo incluyan harían fallar el borrado del permiso por FK.
            migrationBuilder.Sql("DELETE FROM [RolesPermissions] WHERE [PermissionId] = 'cfd3f238-a446-4f4f-81f0-f770974f0cc3';");
            migrationBuilder.Sql("DELETE FROM [SubscriptionPermissions] WHERE [PermissionId] = 'cfd3f238-a446-4f4f-81f0-f770974f0cc3';");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cfd3f238-a446-4f4f-81f0-f770974f0cc3"));

            migrationBuilder.DropColumn(
                name: "CoverLogoId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "MiniLogoId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "Background",
                table: "Apps");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CoverLogoId",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MiniLogoId",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Background",
                table: "Apps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Apps",
                keyColumn: "Id",
                keyValue: new Guid("f4ad89eb-6a0b-427a-8aef-b6bc736884dc"),
                column: "Background",
                value: null);

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "AppId", "Description", "IsPublic", "Key", "Name", "TenantId" },
                values: new object[] { new Guid("cfd3f238-a446-4f4f-81f0-f770974f0cc3"), new Guid("f4ad89eb-6a0b-427a-8aef-b6bc736884dc"), "Can update colors of app in tenant", true, "updatecolors-app", "Can update colors of app", new Guid("882a262c-e1a7-411d-a26e-40c61f3b810c") });

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: new Guid("882a262c-e1a7-411d-a26e-40c61f3b810c"),
                columns: new[] { "CoverLogoId", "MiniLogoId" },
                values: new object[] { null, null });

            migrationBuilder.InsertData(
                table: "RolesPermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("cfd3f238-a446-4f4f-81f0-f770974f0cc3"), new Guid("01e55142-6b8c-4e7e-9d71-1e459d07796d") },
                    { new Guid("cfd3f238-a446-4f4f-81f0-f770974f0cc3"), new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5") }
                });
        }
    }
}
