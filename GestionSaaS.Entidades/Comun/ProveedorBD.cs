namespace GestionSaaS.Entidades.Comun;

/// <summary>Dónde se guardan los datos. Se elige en appsettings.json → "BaseDatos:Proveedor".</summary>
public enum ProveedorBD
{
    /// <summary>Archivo .db local (SQLite). Los datos quedan guardados entre ejecuciones.</summary>
    Sqlite,

    /// <summary>Solo en RAM. Se borra al cerrar la Api.</summary>
    Memoria,

    /// <summary>Servidor SQL Server (para cuando se defina la base real).</summary>
    SqlServer
}
