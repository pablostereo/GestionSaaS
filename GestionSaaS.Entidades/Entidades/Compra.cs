using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GestionSaaS.Entidades.Comun;
using GestionSaaS.Entidades.Enums;

namespace GestionSaaS.Entidades;

public class Compra : IEntidadCliente
{
    public int Id { get; set; }

    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public TipoCompra Tipo { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MontoTotal { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    // Solo las compras de tipo Mercaderia tienen detalle (y suman stock)
    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}
