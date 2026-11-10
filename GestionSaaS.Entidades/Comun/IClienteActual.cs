namespace GestionSaaS.Entidades.Comun;

/// <summary>Indica qué cliente está operando en el request actual.</summary>
public interface IClienteActual
{
    Guid ClienteId { get; }
}
