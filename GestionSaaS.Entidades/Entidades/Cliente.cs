using System.ComponentModel.DataAnnotations;

namespace GestionSaaS.Entidades;

public class Cliente
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(150)]
    public string NombreComercio { get; set; } = string.Empty;

    [Required, MaxLength(256), EmailAddress]
    public string Email { get; set; } = string.Empty;

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    public bool Activo { get; set; } = true;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public ICollection<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();
}
