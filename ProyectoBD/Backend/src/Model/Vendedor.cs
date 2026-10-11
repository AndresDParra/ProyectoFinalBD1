/// <summary>
/// Representa un vendedor en el sistema, que es un tipo de usuario que puede ofrecer productos para la venta.
/// ademas de administrar un inventarios sus productos de los proveedores con los que cuenta
/// el sistema 
/// 
/// aun esta pendiente la relacion con producto. 
/// </summary>

public class Vendedor : Usuario
{
    public Vendedor(
        int idVendedor, // primary key del vendedor en la base de datos
        string nombre,
        string direccion,
        string telefono,
        string cedula,
        string correo,
        DateOnly fechaNacimiento,
        string genero)
        : base(nombre, direccion, telefono, cedula, correo, fechaNacimiento, genero)
    {
        IdVendedor = idVendedor;
    }

    public int IdVendedor { get; private set; }
    public List<Producto> productosVenta { get; } = new(); // representa los productos que el vendedor tiene a la venta  


}