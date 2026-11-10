using System.ComponentModel.DataAnnotations.Schema;

namespace GestionSaaS.Entidades;

public class DetalleVenta
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    [Column(TypeName = "decimal(18,3)")]
    public decimal Cantidad { get; set; }

    // Precio congelado al momento de la venta (no depende de Producto.PrecioVenta)
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
}
