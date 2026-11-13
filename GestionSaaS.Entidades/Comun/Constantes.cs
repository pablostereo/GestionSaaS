using GestionSaaS.Entidades.Enums;

namespace GestionSaaS.Entidades.Comun;

/// <summary>Valores globales compartidos por todas las capas.</summary>
public static class Constantes
{
    // --- Seguridad ---
    /// <summary>Claim del JWT con el Id del cliente al que pertenece el usuario.</summary>
    public const string ClaimCliente = "cliente_id";

    /// <summary>Nombres de rol para [Authorize(Roles = ...)].</summary>
    public const string RolAdmin = nameof(RolUsuario.Admin);
    public const string RolCliente = nameof(RolUsuario.Cliente);

    public const int LargoMinimoContrasena = 6;

    // --- Fechas ---
    /// <summary>Diferencia con UTC (en minutos) para estadísticas si el navegador no la informa. Argentina: UTC-3.</summary>
    public const int UtcOffsetMinutosPorDefecto = -180;

    // --- Base de datos ---
    /// <summary>Usada por "dotnet ef" al generar migraciones.</summary>
    public const string ConexionDiseno =
        "Server=localhost;Database=GestionSaaSDb;Trusted_Connection=True;TrustServerCertificate=True";

    public const string NombreBaseEnMemoria = "GestionSaaSEnMemoria";

    /// <summary>Archivo SQLite por defecto (relativo a la carpeta de la Api).</summary>
    public const string ConexionSqlitePorDefecto = "Data Source=Datos/gestionsaas.db";

    // --- Datos de prueba ---
    /// <summary>Cliente de demostración que se crea al iniciar una base vacía.</summary>
    public static readonly Guid ClienteDemoId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public const string AdminDemoEmail = "admin@gestionsaas.local";
    public const string AdminDemoContrasena = "Admin123!";
    public const string ClienteDemoEmail = "demo@gestionsaas.local";
    public const string ClienteDemoContrasena = "Demo123!";
}
