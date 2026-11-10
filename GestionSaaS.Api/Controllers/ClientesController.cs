using GestionSaaS.Entidades.Comun;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionSaaS.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize(Roles = Constantes.RolAdmin)]
public class ClientesController(ClienteServicio servicio) : ControllerBase
{
    [HttpGet]
    public Task<List<ClienteDto>> Listar(CancellationToken ct) => servicio.ListarAsync(ct);

    [HttpGet("{id:guid}")]
    public Task<ClienteDto> Obtener(Guid id, CancellationToken ct) => servicio.ObtenerAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Crear(ClienteCrearDto dto, CancellationToken ct)
    {
        var cliente = await servicio.CrearAsync(dto, ct);
        return CreatedAtAction(nameof(Obtener), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id:guid}")]
    public Task<ClienteDto> Actualizar(Guid id, ClienteActualizarDto dto, CancellationToken ct) =>
        servicio.ActualizarAsync(id, dto, ct);

    /// <summary>Baja lógica (valor=false) o reactivación (valor=true).</summary>
    [HttpPut("{id:guid}/activo")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromQuery] bool valor, CancellationToken ct)
    {
        await servicio.CambiarEstadoAsync(id, valor, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/contrasena")]
    public async Task<IActionResult> RestablecerContrasena(Guid id, RestablecerContrasenaDto dto, CancellationToken ct)
    {
        await servicio.RestablecerContrasenaAsync(id, dto, ct);
        return NoContent();
    }
}
