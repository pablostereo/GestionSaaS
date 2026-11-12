using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionSaaS.Entidades.Comun;

namespace GestionSaaS.Entidades;

/// <summary>
/// Compra de mercadería para reponer stock. Siempre tiene proveedor y productos:
/// suma stock y actualiza el costo de cada producto.
/// Los pagos que no son mercadería (luz, alquiler…) van en <see cref="Gasto"/>.
/// </summary>
public class Compra : IEntidadCliente
{
    public int Id { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoTotal { get; set; }

    // Nº de factura o remito del proveedor
    [MaxLength(50)]
    public string? NumeroComprobante { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}
