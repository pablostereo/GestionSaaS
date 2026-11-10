using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Servicios;
using GestionSaaS.Entidades.Comun;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionSaaS.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize(Roles = Constantes.RolCliente)]
public class ProductosController(ProductoServicio servicio) : ControllerBase
{
    [HttpGet]
    public Task<List<ProductoDto>> Listar([FromQuery] string? buscar, [FromQuery] bool stockBajo, CancellationToken ct) =>
        servicio.ListarAsync(buscar, stockBajo, ct);

    [HttpGet("{id:int}")]
    public Task<ProductoDto> Obtener(int id, CancellationToken ct) => servicio.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Crear(ProductoGuardarDto dto, CancellationToken ct)
    {
        var producto = await servicio.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = producto.Id }, producto);
    }

    [HttpPut("{id:int}")]
    public Task<ProductoDto> Actualizar(int id, ProductoGuardarDto dto, CancellationToken ct) =>
        servicio.ActualizarAsync(id, dto, ct);

    [HttpPost("{id:int}/ajuste-stock")]
    public Task<ProductoDto> AjustarStock(int id, AjusteStockDto dto, CancellationToken ct) =>
        servicio.AjustarStockAsync(id, dto, ct);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await servicio.EliminarAsync(id, ct);
        return NoContent();
    }
}
