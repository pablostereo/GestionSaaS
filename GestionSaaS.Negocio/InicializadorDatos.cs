using GestionSaaS.Entidades.Datos;
using GestionSaaS.Negocio.Seguridad;
using Microsoft.Extensions.DependencyInjection;

namespace GestionSaaS.Negocio;

public static class InicializadorDatos
{
    /// <summary>Crea/actualiza la base y carga el administrador y el cliente demo si no existen.</summary>
    public static async Task InicializarAsync(IServiceProvider servicios)
    {
        using var scope = servicios.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await ConfiguracionDatos.InicializarAsync(db, Contrasenas.Hashear);
    }
}
