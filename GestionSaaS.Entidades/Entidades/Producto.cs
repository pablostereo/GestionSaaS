using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionSaaS.Entidades.Comun;

namespace GestionSaaS.Entidades;

public class Producto : IEntidadCliente
{
    public int Id { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    [Required, MaxLength(50)]
    public string CodigoSku { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioVenta { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CostoActual { get; set; }

    [Column(TypeName = "decimal(18,3)")]   // 3 decimales: permite vender por kg/litro
    public decimal StockActual { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal StockMinimo { get; set; }

    // Concurrencia optimista: evita que dos ventas simultáneas pisen el stock
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    public ICollection<DetalleCompra> DetallesCompra { get; set; } = new List<DetalleCompra>();
}
