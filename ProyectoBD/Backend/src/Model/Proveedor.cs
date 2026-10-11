/// <summary>
/// representa un proveedor en el sistema, el cual es un tipo de usuario que ofrece productos a los vendedores
/// los provedores ofrecen varios tipos de prodcutos, asi mismo un producto puede ser ofrecido por varios proveedores.
/// ademas los proveedores estan ligados a un empresa y ciudad. 
/// </summary>


public class Proveedor : Usuario
{
    // representa la lista de productos que el proveedor ofrece, junto con el precio que este considera para cada producto
    private List<ProductosOfertados> productosOfertados = new List<ProductosOfertados>();

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

    /// <summary>
    /// Permite al proveedor ofrecer un producto con un precio específico.
    /// </summary>
    /// <param name="producto">El producto a ofrecer.</param>
    /// <param name="precio">El precio del producto.</param>
    public void ofrecerProducto(Producto producto, decimal precio)
    {
        if (producto == null)
        {
            throw new ArgumentNullException(nameof(producto), "El producto no puede ser nulo.");
        }
        if (precio <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(precio), "El precio debe ser mayor que cero.");
        }
        if (productosOfertados.Any(po => po.Producto.IdProducto == producto.IdProducto))
        {
            throw new InvalidOperationException("El producto ya está siendo ofrecido por este proveedor.");
        }

        productosOfertados.Add(new ProductosOfertados(IdProveedor, producto, precio));
    }

    /// <summary>
    /// Permite al proveedor eliminar un producto que estaba ofreciendo.
    /// </summary>
    /// <param name="idProducto">El ID del producto a eliminar.</param>
    public void eliminarProductoOfertado(int idProducto)
    {
        if (!(idProducto <= 0))
        {
            productosOfertados.RemoveAll(po => po.Producto.IdProducto == idProducto);
        }

    }
}