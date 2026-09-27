using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CQ.AuthProvider.DataAccess.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddAppAncestorClosure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppsAncestors",
                columns: table => new
                {
                    AppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AncestorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Depth = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppsAncestors", x => new { x.AppId, x.AncestorId });
                    table.ForeignKey(
                        name: "FK_AppsAncestors_Apps_AncestorId",
                        column: x => x.AncestorId,
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppsAncestors_Apps_AppId",
                        column: x => x.AppId,
                        principalTable: "Apps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppsAncestors_AncestorId",
                table: "AppsAncestors",
                column: "AncestorId");

            // Backfill del cierre para las apps que ya existen: esto no decide nada, solo
            // materializa lo que Apps.FatherAppId ya dice.
            //
            // El corte de profundidad y el GROUP BY + MIN estan porque no se asume que el arbol
            // este sano: AppRepository.GetAncestorIdsAsync ya se defendia de ciclos en los datos,
            // asi que aca tampoco se confia. Sin eso, un ciclo preexistente haria recursion
            // infinita o violaria la PK con pares duplicados.
            migrationBuilder.Sql(@"
WITH chain AS (
    SELECT [Id] AS app_id, [FatherAppId] AS ancestor_id, 1 AS depth
    FROM [Apps]
    WHERE [FatherAppId] IS NOT NULL
    UNION ALL
    SELECT c.app_id, a.[FatherAppId], c.depth + 1
    FROM chain c
    INNER JOIN [Apps] a ON a.[Id] = c.ancestor_id
    WHERE a.[FatherAppId] IS NOT NULL
      AND c.depth < 32
      AND a.[FatherAppId] <> c.app_id
)
INSERT INTO [AppsAncestors] ([AppId], [AncestorId], [Depth])
SELECT app_id, ancestor_id, MIN(depth)
FROM chain
GROUP BY app_id, ancestor_id;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppsAncestors");
        }
    }
}
