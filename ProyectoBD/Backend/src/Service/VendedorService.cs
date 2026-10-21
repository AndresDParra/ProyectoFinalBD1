/// <summary>
/// representa un servicio para el manejo de los vendedores.
/// los vendedores al ser usuarios esporadicos solo tienen la capacidad de realizar consultas a la base de datos
/// en el objetivo de esta clase es definir los diferentes tipos de consultas que puede realizar un vendedor
/// </summary>

public class VendedorService
{

    private readonly IProductoDAO _productoDAO;
    private readonly IProductoOfertadoDAO _productoOfertadoDAO;

    private readonly IProveedorDAO _proveedorDAO;

    public VendedorService(IProductoDAO productoDAO, IProductoOfertadoDAO productoOfertadoDAO, IProveedorDAO proveedorDAO)
    {
        _productoDAO = productoDAO;
        _productoOfertadoDAO = productoOfertadoDAO;
        _proveedorDAO = proveedorDAO;
    }

    // metodo para consultar todos los proveedores
    // metodo para consultar todos los productos
    // metodo para consultar todos los productos ofertados por id producto
    // metodo para consultar todos los productos ofertados por id proveedor
    // metodo para consultar productos ofertados en un rango de precios y su proveedor (posible JOIN)
    // metodo para consultar productos apartir de su categoria
    // metodo para consultar productos apartir de su nombre
    // metodo para consultar proveedores apartir de su ciudad
    
}