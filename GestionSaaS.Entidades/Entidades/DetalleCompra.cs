using System.ComponentModel.DataAnnotations.Schema;

namespace GestionSaaS.Entidades;

public class DetalleCompra
{
    public int Id { get; set; }

    public int CompraId { get; set; }
    public Compra Compra { get; set; } = null!;

    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    [Column(TypeName = "decimal(18,3)")]
    public decimal Cantidad { get; set; }

    // Costo pagado en esta compra (queda congelado aunque después cambie Producto.CostoActual)
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostoUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
}
