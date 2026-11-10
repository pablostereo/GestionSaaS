using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Negocio.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GestionSaaS.Api.Infraestructura;

/// <summary>Arma el JWT con los datos del usuario logueado.</summary>
public class GeneradorToken(IOptions<ConfiguracionJwt> opciones)
{
    public LoginRespuestaDto Generar(UsuarioSesionDto usuario)
    {
        var cfg = opciones.Value;
        var expira = DateTime.UtcNow.AddHours(cfg.HorasExpiracion);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new("name", usuario.Nombre),
            new("role", usuario.Rol.ToString())
        };
        if (usuario.ClienteId is Guid clienteId)
            claims.Add(new Claim(Constantes.ClaimCliente, clienteId.ToString()));

        var token = new JwtSecurityToken(
            issuer: cfg.Emisor,
            audience: cfg.Emisor,
            claims: claims,
            expires: expira,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg.Clave)), SecurityAlgorithms.HmacSha256));

        return new LoginRespuestaDto(new JwtSecurityTokenHandler().WriteToken(token), expira, usuario);
    }
}
