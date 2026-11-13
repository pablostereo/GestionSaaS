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

    // Costo del producto al momento de la venta (congelado: no cambia si después cambia Producto.CostoActual).
    // Es la base del costo de mercadería vendida y de la ganancia real.
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostoUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
}
