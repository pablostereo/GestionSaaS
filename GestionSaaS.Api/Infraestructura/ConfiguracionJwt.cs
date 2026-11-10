namespace GestionSaaS.Api.Infraestructura;

/// <summary>Sección "Jwt" de appsettings.json.</summary>
public class ConfiguracionJwt
{
    public string Clave { get; set; } = string.Empty;
    public string Emisor { get; set; } = "GestionSaaS";
    public int HorasExpiracion { get; set; } = 8;
}
