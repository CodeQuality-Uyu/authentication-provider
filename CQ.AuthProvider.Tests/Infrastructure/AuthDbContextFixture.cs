using CQ.AuthProvider.DataAccess.EfCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CQ.AuthProvider.Tests.Infrastructure;

/// <summary>
/// Un <see cref="AuthDbContext"/> contra SQLite en memoria, con el esquema y el seed creados.
/// </summary>
/// <remarks>
/// Se usa SQLite y no el provider InMemory de EF porque InMemory no traduce SQL: no ejercita los
/// subqueries correlacionados ni los joins con los que se resuelve el alcance efectivo, que es
/// justamente lo que estos tests tienen que probar. Con SQLite, si una query no traduce, el test
/// falla.
/// <para>
/// Lo que SQLite no cubre: el backfill recursivo de AppsAncestors, que está escrito en dialecto
/// Postgres y SQL Server dentro de las migraciones. Acá el esquema se crea con EnsureCreated, que
/// no corre migraciones — así que ese SQL sigue sin ejecutarse nunca en CI.
/// </para>
/// </remarks>
public sealed class AuthDbContextFixture
    : IDisposable
{
    private readonly SqliteConnection _connection;

    public AuthDbContextFixture()
    {
        // La conexión se mantiene abierta a propósito: en SQLite, "in memory" vive mientras haya
        // una conexión abierta. Si se cierra, la base desaparece.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AuthDbContext(options);

        Context.Database.EnsureCreated();
    }

    public AuthDbContext Context { get; }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
