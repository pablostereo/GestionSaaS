using GestionSaaS.Entidades.Datos;
using GestionSaaS.Entidades.Enums;
using GestionSaaS.Negocio.Dtos;
using GestionSaaS.Negocio.Excepciones;
using GestionSaaS.Negocio.Seguridad;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Negocio.Servicios;

public class AuthServicio(AppDbContext db)
{
    private const string CredencialesInvalidas = "Email o contraseña incorrectos.";

    /// <summary>Valida las credenciales y devuelve los datos del usuario para armar la sesión.</summary>
    public async Task<UsuarioSesionDto> ValidarAsync(LoginDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Contrasena))
            throw new NoAutorizadoException(CredencialesInvalidas);

        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.Include(u => u.Cliente).FirstOrDefaultAsync(u => u.Email == email, ct);

        // Mismo mensaje si no existe o si la contraseña está mal (no revela qué emails existen)
        if (usuario is null || !Contrasenas.Verificar(usuario.ContrasenaHash, dto.Contrasena))
            throw new NoAutorizadoException(CredencialesInvalidas);

        if (!usuario.Activo)
            throw new NoAutorizadoException("El usuario está deshabilitado.");

        if (usuario.Rol == RolUsuario.Cliente && usuario.Cliente is not { Activo: true })
            throw new NoAutorizadoException("El cliente está dado de baja. Contacte al administrador.");

        usuario.UltimoIngreso = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return new UsuarioSesionDto(usuario.Id, usuario.Email, usuario.Nombre, usuario.Rol,
            usuario.ClienteId, usuario.Cliente?.NombreComercio);
    }
}
