using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Enums;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionSaaS.Api.Controllers;

/// <summary>Gastos / egresos operativos (no tocan stock).</summary>
[ApiController]
[Route("api/gastos")]
[Authorize(Roles = Constantes.RolCliente)]
public class GastosController(GastoServicio servicio) : ControllerBase
{
    [HttpGet]
    public Task<List<GastoDto>> Listar([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
                                       [FromQuery] CategoriaGasto? categoria, CancellationToken ct) =>
        servicio.ListarAsync(desde, hasta, categoria, ct);

    [HttpGet("{id:int}")]
    public Task<GastoDto> Obtener(int id, CancellationToken ct) => servicio.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<GastoDto>> Registrar(GastoGuardarDto dto, CancellationToken ct)
    {
        var gasto = await servicio.RegistrarAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = gasto.Id }, gasto);
    }

    [HttpPut("{id:int}")]
    public Task<GastoDto> Actualizar(int id, GastoGuardarDto dto, CancellationToken ct) =>
        servicio.ActualizarAsync(id, dto, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await servicio.EliminarAsync(id, ct);
        return NoContent();
    }
}
