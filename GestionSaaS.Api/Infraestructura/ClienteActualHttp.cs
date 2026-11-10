using GestionSaaS.Entidades.Comun;

namespace GestionSaaS.Api.Infraestructura;

/// <summary>Toma el cliente del claim "cliente_id" del JWT. Sin token (o admin) => Guid.Empty.</summary>
public class ClienteActualHttp(IHttpContextAccessor http) : IClienteActual
{
    public Guid ClienteId =>
        Guid.TryParse(http.HttpContext?.User.FindFirst(Constantes.ClaimCliente)?.Value, out var id)
            ? id
            : Guid.Empty;
}
