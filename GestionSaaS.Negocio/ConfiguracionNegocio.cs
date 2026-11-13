using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Datos;
using GestionSaaS.Negocio.Servicios;
using Microsoft.Extensions.DependencyInjection;

namespace GestionSaaS.Negocio;

public static class ConfiguracionNegocio
{
    /// <summary>Registra los servicios de negocio y la capa de datos.</summary>
    public static IServiceCollection AddNegocio(
        this IServiceCollection services, ProveedorBD proveedor, string? cadenaConexion, string carpetaBase)
    {
        services.AddDatos(proveedor, cadenaConexion, carpetaBase);

        services.AddScoped<AuthServicio>();
        services.AddScoped<ClienteServicio>();
        services.AddScoped<ProductoServicio>();
        services.AddScoped<VentaServicio>();
        services.AddScoped<ProveedorServicio>();
        services.AddScoped<CompraServicio>();
        services.AddScoped<GastoServicio>();
        services.AddScoped<EstadisticaServicio>();

        return services;
    }
}
