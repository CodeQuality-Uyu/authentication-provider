using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CQ.AuthProvider.Postgres.Migrations.Migrations.IdentityDb
{
    /// <inheritdoc />
    public partial class AddGoogleIdentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoogleIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoogleSub = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleIdentities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoogleIdentities_GoogleSub",
                table: "GoogleIdentities",
                column: "GoogleSub",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoogleIdentities");
        }
    }
}
