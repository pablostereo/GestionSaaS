using GestionSaaS.Entidades.Comun;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GestionSaaS.Entidades.Datos;

/// <summary>
/// Solo lo usa "dotnet ef" (migraciones). Permite generar migraciones
/// desde este proyecto sin depender de la Api.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(Constantes.ConexionDiseno)
            .Options;

        return new AppDbContext(options, new SinCliente());
    }

    private sealed class SinCliente : IClienteActual
    {
        public Guid ClienteId => Guid.Empty;
    }
}
