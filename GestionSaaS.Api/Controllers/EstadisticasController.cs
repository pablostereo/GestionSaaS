using GestionSaaS.Entidades.Comun;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionSaaS.Api.Controllers;

/// <summary>Estadísticas e inteligencia de negocio del comercio.</summary>
[ApiController]
[Route("api/estadisticas")]
[Authorize(Roles = Constantes.RolCliente)]
public class EstadisticasController(EstadisticaServicio servicio) : ControllerBase
{
    /// <param name="desde">yyyy-MM-dd. Por defecto: últimos 30 días.</param>
    /// <param name="hasta">yyyy-MM-dd, incluido. Por defecto: hoy.</param>
    /// <param name="utcOffset">Minutos de diferencia con UTC del navegador, para agrupar por día y hora. Argentina: -180 (valor por defecto).</param>
    [HttpGet]
    public Task<EstadisticasDto> Obtener([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
                                         [FromQuery] int? utcOffset, CancellationToken ct) =>
        servicio.ObtenerAsync(desde, hasta, utcOffset, ct);
}
