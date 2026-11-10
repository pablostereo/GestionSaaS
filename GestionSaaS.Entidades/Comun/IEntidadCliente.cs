namespace GestionSaaS.Entidades.Comun;

/// <summary>Marca a las entidades que pertenecen a un cliente (multi-tenant por columna).</summary>
public interface IEntidadCliente
{
    Guid ClienteId { get; set; }
}
