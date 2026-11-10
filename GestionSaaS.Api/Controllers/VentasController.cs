using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Servicios;
using GestionSaaS.Entidades.Comun;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionSaaS.Api.Controllers;

[ApiController]
[Route("api/ventas")]
[Authorize(Roles = Constantes.RolCliente)]
public class VentasController(VentaServicio servicio) : ControllerBase
{
    [HttpGet]
    public Task<List<VentaDto>> Listar([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, CancellationToken ct) =>
        servicio.ListarAsync(desde, hasta, ct);

    [HttpGet("{id:int}")]
    public Task<VentaDto> Obtener(int id, CancellationToken ct) => servicio.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<VentaDto>> Registrar(VentaCrearDto dto, CancellationToken ct)
    {
        var venta = await servicio.RegistrarAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = venta.Id }, venta);
    }
}
