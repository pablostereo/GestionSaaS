using GestionSaaS.Entidades.Comun;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace GestionSaaS.Entidades.Datos;

public static class ConfiguracionDatos
{
    /// <summary>
    /// Registra el AppDbContext según el proveedor elegido.
    /// </summary>
    /// <param name="carpetaBase">Carpeta desde la que se resuelven rutas relativas del archivo SQLite.</param>
    public static IServiceCollection AddDatos(
        this IServiceCollection services, ProveedorBD proveedor, string? cadenaConexion, string carpetaBase)
    {
        switch (proveedor)
        {
            case ProveedorBD.Sqlite:
                var cadena = PrepararSqlite(cadenaConexion ?? Constantes.ConexionSqlitePorDefecto, carpetaBase);
                services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(cadena));
                break;

            case ProveedorBD.Memoria:
                services.AddDbContext<AppDbContext>(opt => opt
                    .UseInMemoryDatabase(Constantes.NombreBaseEnMemoria)
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
                break;

            case ProveedorBD.SqlServer:
                if (string.IsNullOrWhiteSpace(cadenaConexion))
                    throw new InvalidOperationException("Falta la connection string de SQL Server.");
                services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(cadenaConexion));
                break;
        }

        return services;
    }

    /// <summary>Crea la base si no existe y carga datos demo si está vacía.</summary>
    /// <param name="hashear">Función para hashear las contraseñas de los usuarios demo.</param>
    public static async Task InicializarAsync(AppDbContext db, Func<string, string> hashear)
    {
        if (db.Database.IsSqlServer())
            await db.Database.MigrateAsync();      // SQL Server: usa las migraciones
        else
        {
            if (db.Database.IsSqlite())
                RespaldarSiCambioLaEstructura(db);

            await db.Database.EnsureCreatedAsync(); // SQLite / Memoria: crea las tablas desde el modelo
        }

        await DatosDemo.CargarAsync(db, hashear);
    }

    /// <summary>
    /// SQLite no usa migraciones: si las entidades cambiaron desde que se creó el .db,
    /// lo mueve a Datos/respaldo/ para que se cree uno nuevo con la estructura actual.
    /// </summary>
    private static void RespaldarSiCambioLaEstructura(AppDbContext db)
    {
        var archivo = new SqliteConnectionStringBuilder(db.Database.GetConnectionString()).DataSource;
        var archivoFirma = archivo + ".estructura";
        var firmaActual = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(db.Database.GenerateCreateScript())));

        if (File.Exists(archivo))
        {
            var firmaAnterior = File.Exists(archivoFirma) ? File.ReadAllText(archivoFirma) : "";
            if (firmaAnterior == firmaActual)
                return;

            SqliteConnection.ClearAllPools();
            var carpetaRespaldo = Path.Combine(Path.GetDirectoryName(archivo)!, "respaldo");
            Directory.CreateDirectory(carpetaRespaldo);
            var sufijo = DateTime.Now.ToString("yyyyMMdd-HHmmss");

            foreach (var extension in new[] { "", "-wal", "-shm" })
                if (File.Exists(archivo + extension))
                    File.Move(archivo + extension,
                        Path.Combine(carpetaRespaldo, $"{Path.GetFileName(archivo)}{extension}.{sufijo}"));
        }

        File.WriteAllText(archivoFirma, firmaActual);
    }

    /// <summary>Convierte la ruta del .db en absoluta y crea la carpeta si no existe.</summary>
    private static string PrepararSqlite(string cadenaConexion, string carpetaBase)
    {
        var builder = new SqliteConnectionStringBuilder(cadenaConexion);

        if (!Path.IsPathRooted(builder.DataSource))
            builder.DataSource = Path.GetFullPath(Path.Combine(carpetaBase, builder.DataSource));

        Directory.CreateDirectory(Path.GetDirectoryName(builder.DataSource)!);
        return builder.ToString();
    }
}
