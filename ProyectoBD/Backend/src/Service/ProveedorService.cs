/// <summary>
/// representa un servicio para el manejo de los proveedores, productos y productos ofertados
/// un proveedor al ser un usuario tradicional tiene la capacidad de ejecutar CRUDs sobre sus productos ofertados
/// y realizar operaciones limitadas sobre los productos
/// </summary>

public class ProveedorService
{
    private readonly int _idProveedorAutenticado; // evita que un proveedor pueda realizar operaciones sobre otro proveedor

    private readonly ProveedorDAO _proveedorDAO;

    private readonly ProductoDAO _productoDAO;

    private readonly ProductoOfertadoDAO _productoOfertadoDao;

    public ProveedorService(
        int idProveedorAutenticado,
        ProveedorDAO proveedorDAO,
        ProductoDAO productoDAO,
        ProductoOfertadoDAO productoOfertadoDao)
    {
        if (!proveedorDAO.Existe(idProveedorAutenticado))
        {
            throw new KeyNotFoundException($"No existe el proveedor {idProveedorAutenticado}.");
        }

        _idProveedorAutenticado = idProveedorAutenticado;
        _proveedorDAO = proveedorDAO;
        _productoDAO = productoDAO;
        _productoOfertadoDao = productoOfertadoDao;
    }

    // operaciones sobre los productos ofertados

    public bool CrearProductoOfertado(Producto producto, decimal precio, int tiempoEntrega = 0)
    {
        ArgumentNullException.ThrowIfNull(producto);

        if (!_productoDAO.Existe(producto.IdProducto)) // Verifica que el producto exista
        {
            return false;
        }

        ProductoOfertado nuevoProductoOfertado = new ProductoOfertado(
            _idProveedorAutenticado,
            producto,
            precio,
            tiempoEntrega);

        return _productoOfertadoDao.Crear(nuevoProductoOfertado);
    }

    public bool EliminarProductoOfertado(int idProducto)
    {
        if (!_productoDAO.Existe(idProducto))
        {
            return false;
        }

        return _productoOfertadoDao.Eliminar(_idProveedorAutenticado, idProducto);
    }

    public List<ProductoOfertado> ConsultarProductoOfertadoPorProveedor()
    {
        return _productoOfertadoDao.ConsultarIdProveedor(_idProveedorAutenticado);
    }

    public ProductoOfertado ConsultarProducOfertadoPorIdProducto(int idProducto)
    {
        if (!_productoDAO.Existe(idProducto))
        {
            throw new KeyNotFoundException($"No existe un producto con el ID {idProducto}.");
        }

        return _productoOfertadoDao.ConsultarIdProducto(idProducto);
    }

    public bool ActualizarPrecioProductoOfertado(int idProducto, decimal nuevoPrecio)
    {
        if (!_productoDAO.Existe(idProducto))
        {
            throw new KeyNotFoundException($"No existe un producto con el ID {idProducto}.");
        }

        return _productoOfertadoDao.ActualizarPrecio(_idProveedorAutenticado, idProducto, nuevoPrecio);
    }
}