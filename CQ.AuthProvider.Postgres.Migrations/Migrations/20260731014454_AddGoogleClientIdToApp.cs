using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CQ.AuthProvider.Postgres.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleClientIdToApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleClientId",
                table: "Apps",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Apps",
                keyColumn: "Id",
                keyValue: new Guid("f4ad89eb-6a0b-427a-8aef-b6bc736884dc"),
                column: "GoogleClientId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleClientId",
                table: "Apps");
        }
    }
}
