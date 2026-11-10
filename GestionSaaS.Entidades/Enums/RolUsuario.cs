namespace GestionSaaS.Entidades.Enums;

public enum RolUsuario
{
    /// <summary>Administrador del sistema: gestiona clientes. No pertenece a ningún cliente.</summary>
    Admin = 1,

    /// <summary>Usuario de un comercio: gestiona su stock, ventas y compras.</summary>
    Cliente = 2
}
