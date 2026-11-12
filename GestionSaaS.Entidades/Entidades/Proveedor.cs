using System.ComponentModel.DataAnnotations;
using GestionSaaS.Entidades.Comun;

namespace GestionSaaS.Entidades;

/// <summary>Proveedor de mercadería de un cliente.</summary>
public class Proveedor : IEntidadCliente
{
    public int Id { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Cuit { get; set; }

    [MaxLength(50)]
    public string? Telefono { get; set; }

    [MaxLength(256)]
    public string? Email { get; set; }

    // No se borran (tienen compras asociadas): se desactivan
    public bool Activo { get; set; } = true;

    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
