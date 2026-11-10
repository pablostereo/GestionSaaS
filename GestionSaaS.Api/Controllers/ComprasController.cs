using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Servicios;
using GestionSaaS.Entidades.Comun;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionSaaS.Api.Controllers;

[ApiController]
[Route("api/compras")]
[Authorize(Roles = Constantes.RolCliente)]
public class ComprasController(CompraServicio servicio) : ControllerBase
{
    [HttpGet]
    public Task<List<CompraDto>> Listar([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, CancellationToken ct) =>
        servicio.ListarAsync(desde, hasta, ct);

    [HttpGet("{id:int}")]
    public Task<CompraDto> Obtener(int id, CancellationToken ct) => servicio.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<CompraDto>> Registrar(CompraCrearDto dto, CancellationToken ct)
    {
        var compra = await servicio.RegistrarAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = compra.Id }, compra);
    }
}
