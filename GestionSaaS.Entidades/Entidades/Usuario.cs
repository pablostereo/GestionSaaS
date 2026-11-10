using System.ComponentModel.DataAnnotations;
using GestionSaaS.Entidades.Enums;

namespace GestionSaaS.Entidades;

public class Usuario
{
    public int Id { get; set; }

    [Required, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Hash PBKDF2 (nunca la contraseña en texto plano).</summary>
    [Required, MaxLength(500)]
    public string ContrasenaHash { get; set; } = string.Empty;

    public RolUsuario Rol { get; set; }

    /// <summary>Null para administradores; obligatorio para usuarios de un cliente.</summary>
    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    public DateTime? UltimoIngreso { get; set; }
}
