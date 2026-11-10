using GestionSaaS.Entidades;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Negocio.Excepciones;
using Microsoft.AspNetCore.Identity;

namespace GestionSaaS.Negocio.Seguridad;

/// <summary>Hash y verificación de contraseñas (PBKDF2, el mismo algoritmo de ASP.NET Identity).</summary>
public static class Contrasenas
{
    private static readonly PasswordHasher<Usuario> Hasher = new();

    public static string Hashear(string contrasena) => Hasher.HashPassword(null!, contrasena);

    public static bool Verificar(string hash, string contrasena) =>
        Hasher.VerifyHashedPassword(null!, hash, contrasena) != PasswordVerificationResult.Failed;

    public static void Validar(string? contrasena)
    {
        if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < Constantes.LargoMinimoContrasena)
            throw new NegocioException($"La contraseña debe tener al menos {Constantes.LargoMinimoContrasena} caracteres.");
    }
}
