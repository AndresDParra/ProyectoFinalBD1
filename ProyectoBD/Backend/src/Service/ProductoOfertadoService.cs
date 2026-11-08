/// <summary>
/// representa un servicio para el manejo de los proveedores, productos y productos ofertados
/// un proveedor al ser un usuario tradicional tiene la capacidad de ejecutar CRUDs sobre sus productos ofertados
/// y realizar operaciones limitadas sobre los productos
/// </summary>

public class ProveedorService
{
    private readonly AutorizacionService _autorizacionService;
    private readonly ProductoDAO _productoDAO;

    private readonly ProductoOfertadoDAO _productoOfertadoDao;

    public ProveedorService(
        AutorizacionService autorizacionService,
        ProductoDAO productoDAO,
        ProductoOfertadoDAO productoOfertadoDao)
    {
        _autorizacionService = autorizacionService;
        _productoDAO = productoDAO;
        _productoOfertadoDao = productoOfertadoDao;
    }

    // operaciones sobre los productos ofertados

    public bool CrearProductoOfertado(int idSolicitante, Producto producto, decimal precio, int tiempoEntrega = 0)
    {
        if (producto == null)
        {
            throw new ArgumentNullException(nameof(producto), "El producto no puede ser nulo.");
        }

        _autorizacionService.ValidarRol(idSolicitante, RolUsuario.Proveedor);

        if (!_productoDAO.Existe(producto.IdProducto)) // Verifica que el producto exista
        {
            return false;
        }

        ProductoOfertado nuevoProductoOfertado = new ProductoOfertado(
            idSolicitante,
            producto,
            precio,
            tiempoEntrega);

        return _productoOfertadoDao.Crear(nuevoProductoOfertado);
    }

    public bool EliminarProductoOfertado(int idSolicitante, int idProducto)
    {
        _autorizacionService.ValidarRol(idSolicitante, RolUsuario.Proveedor);

        if (!_productoDAO.Existe(idProducto))
        {
            return false;
        }

        return _productoOfertadoDao.Eliminar(idSolicitante, idProducto);
    }

    public List<ProductoOfertado> ConsultarProductoOfertadoPorProveedor(int idSolicitante)
    {
        _autorizacionService.ValidarRol(idSolicitante, RolUsuario.Proveedor);

        return _productoOfertadoDao.ConsultarIdProveedor(idSolicitante);
    }

    public ProductoOfertado ConsultarProducOfertadoPorIdProducto(int idProducto)
    {
        if (!_productoDAO.Existe(idProducto))
        {
            throw new KeyNotFoundException($"No existe un producto con el ID {idProducto}.");
        }

        return _productoOfertadoDao.ConsultarIdProducto(idProducto);
    }

    public bool ActualizarPrecioProductoOfertado(int idSolicitante, int idProducto, decimal nuevoPrecio)
    {
        _autorizacionService.ValidarRol(idSolicitante, RolUsuario.Proveedor);

        if (nuevoPrecio < 0)
        {
            throw new ArgumentException("El precio no puede ser negativo.");
        }

        if (!_productoDAO.Existe(idProducto))
        {
            throw new KeyNotFoundException($"No existe un producto con el ID {idProducto}.");
        }

        return _productoOfertadoDao.ActualizarPrecio(idSolicitante, idProducto, nuevoPrecio);
    }
}