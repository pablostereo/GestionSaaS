using GestionSaaS.Api.Infraestructura;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionSaaS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthServicio servicio, GeneradorToken generadorToken) : ControllerBase
{
    /// <summary>Devuelve un token JWT. Usarlo en el botón "Authorize" de Swagger.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<LoginRespuestaDto> Login(LoginDto dto, CancellationToken ct) =>
        generadorToken.Generar(await servicio.ValidarAsync(dto, ct));

    /// <summary>Datos del usuario del token actual.</summary>
    [HttpGet("yo")]
    public object Yo() => User.Claims.ToDictionary(c => c.Type, c => c.Value);
}
