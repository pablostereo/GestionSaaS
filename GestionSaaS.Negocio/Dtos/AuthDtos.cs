using GestionSaaS.Entidades.Enums;

namespace GestionSaaS.Negocio.Dtos;

public record LoginDto(string Email, string Contrasena);

/// <summary>Datos del usuario logueado (con esto la Api arma el token).</summary>
public record UsuarioSesionDto(int Id, string Email, string Nombre, RolUsuario Rol, Guid? ClienteId, string? NombreComercio);

public record LoginRespuestaDto(string Token, DateTime Expira, UsuarioSesionDto Usuario);
