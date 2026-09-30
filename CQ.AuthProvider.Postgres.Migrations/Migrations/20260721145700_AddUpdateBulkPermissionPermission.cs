using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CQ.AuthProvider.Postgres.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateBulkPermissionPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // El permiso "updatebulk-permission" fue cargado a mano en algunas bases antes de que
            // existiera esta migracion, con un Id distinto al del seed. Un InsertData plano revienta
            // ahi con 23505 sobre el indice unico de Key. Por eso el seed se hace idempotente:
            // si la fila ya existe con otro Id, se reapunta a los hijos y se la normaliza al Id del
            // seed, para que las migraciones y el snapshot siguientes sigan siendo validos.
            migrationBuilder.Sql(@"
DO $$
DECLARE
    seed_id uuid := '00000000-0000-0000-0000-000000000005';
    existing_id uuid;
BEGIN
    SELECT ""Id"" INTO existing_id FROM ""Permissions"" WHERE ""Key"" = 'updatebulk-permission';

    IF existing_id IS NULL THEN
        INSERT INTO ""Permissions"" (""Id"", ""AppId"", ""Description"", ""IsPublic"", ""Key"", ""Name"", ""TenantId"")
        VALUES (
            seed_id,
            'f4ad89eb-6a0b-427a-8aef-b6bc736884dc',
            'Can update several permissions at once',
            true,
            'updatebulk-permission',
            'Update permission in bulk',
            '882a262c-e1a7-411d-a26e-40c61f3b810c');

    ELSIF existing_id <> seed_id THEN
        -- Se libera la Key para poder insertar la fila del seed y recien despues se migran los hijos.
        UPDATE ""Permissions"" SET ""Key"" = 'updatebulk-permission__migrated' WHERE ""Id"" = existing_id;

        INSERT INTO ""Permissions"" (""Id"", ""AppId"", ""Description"", ""IsPublic"", ""Key"", ""Name"", ""TenantId"")
        VALUES (
            seed_id,
            'f4ad89eb-6a0b-427a-8aef-b6bc736884dc',
            'Can update several permissions at once',
            true,
            'updatebulk-permission',
            'Update permission in bulk',
            '882a262c-e1a7-411d-a26e-40c61f3b810c');

        DELETE FROM ""RolesPermissions"" rp
        WHERE rp.""PermissionId"" = existing_id
          AND EXISTS (SELECT 1 FROM ""RolesPermissions"" x WHERE x.""RoleId"" = rp.""RoleId"" AND x.""PermissionId"" = seed_id);

        UPDATE ""RolesPermissions"" SET ""PermissionId"" = seed_id WHERE ""PermissionId"" = existing_id;

        DELETE FROM ""SubscriptionPermissions"" sp
        WHERE sp.""PermissionId"" = existing_id
          AND EXISTS (SELECT 1 FROM ""SubscriptionPermissions"" x WHERE x.""SubscriptionId"" = sp.""SubscriptionId"" AND x.""PermissionId"" = seed_id);

        UPDATE ""SubscriptionPermissions"" SET ""PermissionId"" = seed_id WHERE ""PermissionId"" = existing_id;

        DELETE FROM ""Permissions"" WHERE ""Id"" = existing_id;
    END IF;

    INSERT INTO ""RolesPermissions"" (""PermissionId"", ""RoleId"")
    VALUES
        (seed_id, '4579a206-b6c7-4d58-9d36-c3e0923041b5'),
        (seed_id, 'cf4a209a-8dbd-4dac-85d9-ed899424b49e')
    ON CONFLICT DO NOTHING;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000005"), new Guid("4579a206-b6c7-4d58-9d36-c3e0923041b5") });

            migrationBuilder.DeleteData(
                table: "RolesPermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("00000000-0000-0000-0000-000000000005"), new Guid("cf4a209a-8dbd-4dac-85d9-ed899424b49e") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"));
        }
    }
}
