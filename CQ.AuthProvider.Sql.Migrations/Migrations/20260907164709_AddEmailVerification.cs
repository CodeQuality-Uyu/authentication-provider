using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CQ.AuthProvider.DataAccess.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EmailVerifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerifications", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("0ee82ee9-f480-4b13-ad68-579dc83dfa0d"),
                column: "IsEmailVerified",
                value: false);

            // Backfill: las cuentas creadas antes de que existiera este flujo nunca pasaron por
            // verificación de email. Sin esto, SessionService las bloquearía en el próximo login
            // (IsEmailVerified default = false) exigiéndoles verificar un email que ya usaban de
            // antes. Las cuentas nuevas siguen naciendo sin verificar (Account.New no toca esta
            // columna), así que esto solo afecta a las filas que ya existían al migrar.
            migrationBuilder.Sql("UPDATE [Accounts] SET [IsEmailVerified] = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailVerifications");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "Accounts");
        }
    }
}
