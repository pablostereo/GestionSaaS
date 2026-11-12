using GestionSaaS.Entidades.Comun;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionSaaS.Api.Controllers;

[ApiController]
[Route("api/proveedores")]
[Authorize(Roles = Constantes.RolCliente)]
public class ProveedoresController(ProveedorServicio servicio) : ControllerBase
{
    [HttpGet]
    public Task<List<ProveedorDto>> Listar([FromQuery] string? buscar, [FromQuery] bool soloActivos, CancellationToken ct) =>
        servicio.ListarAsync(buscar, soloActivos, ct);

    [HttpGet("{id:int}")]
    public Task<ProveedorDto> Obtener(int id, CancellationToken ct) => servicio.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<ProveedorDto>> Crear(ProveedorGuardarDto dto, CancellationToken ct)
    {
        var proveedor = await servicio.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = proveedor.Id }, proveedor);
    }

    [HttpPut("{id:int}")]
    public Task<ProveedorDto> Actualizar(int id, ProveedorGuardarDto dto, CancellationToken ct) =>
        servicio.ActualizarAsync(id, dto, ct);

    [HttpPut("{id:int}/activo")]
    public Task<ProveedorDto> CambiarEstado(int id, [FromQuery] bool valor, CancellationToken ct) =>
        servicio.CambiarEstadoAsync(id, valor, ct);
}
