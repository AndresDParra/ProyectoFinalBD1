/// <summary>
/// representa un proveedor en el sistema, el cual es un tipo de usuario que ofrece productos a los vendedores
/// los provedores ofrecen varios tipos de prodcutos, asi mismo un producto puede ser ofrecido por varios proveedores.
/// ademas los proveedores estan ligados a un empresa y ciudad. 
/// </summary>


public class Proveedor : Usuario
{
    // representa la lista de productos que el proveedor ofrece, junto con el precio que este considera para cada producto
    private List<ProductoOfertado> productosOfertados = new List<ProductoOfertado>();

    public Proveedor(
        int idProveedor, // primary key del proveedor en la base de datos
        string nombreEmpresa, // representa la empresa del proveedor
        string ciudad, //representa la ciudad donde esta ubicado el proveedor



        string nombre,
        string direccion,
        string telefono,
        string cedula,
        string correo,
        DateOnly fechaNacimiento,
        string genero)
        : base(nombre, direccion, telefono, cedula, correo, fechaNacimiento, genero)
    {
        IdProveedor = idProveedor;
        NombreEmpresa = nombreEmpresa;
        Ciudad = ciudad;
    }

    public int IdProveedor { get; private set; }
    public string NombreEmpresa { get; private set; }
    public string Ciudad { get; private set; }
}