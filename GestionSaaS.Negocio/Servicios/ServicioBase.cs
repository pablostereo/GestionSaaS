using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Datos;
using GestionSaaS.Negocio.Excepciones;
using Microsoft.EntityFrameworkCore;

namespace GestionSaaS.Negocio.Servicios;

public abstract class ServicioBase(AppDbContext db, IClienteActual clienteActual)
{
    protected AppDbContext Db { get; } = db;
    protected Guid ClienteId => clienteActual.ClienteId;

    /// <summary>Verifica que haya un cliente identificado, existente y activo.</summary>
    protected async Task ValidarClienteAsync(CancellationToken ct)
    {
        if (ClienteId == Guid.Empty)
            throw new NoAutorizadoException("La sesión no pertenece a ningún cliente.");

        var activo = await Db.Clientes.AnyAsync(c => c.Id == ClienteId && c.Activo, ct);
        if (!activo)
            throw new NoAutorizadoException("El cliente está dado de baja. Contacte al administrador.");
    }
}
