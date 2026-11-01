public class ProductoService
{
    private readonly AutorizacionService _autorizacionService;
    private readonly IProductoDAO _productoDAO;
    private readonly IProductoOfertadoDAO _productoOfertadoDAO;

    public ProductoService(
        AutorizacionService autorizacionService,
        IProductoDAO productoDAO,
        IProductoOfertadoDAO productoOfertadoDAO)
    {
        _autorizacionService = autorizacionService;
        _productoDAO = productoDAO;
        _productoOfertadoDAO = productoOfertadoDAO;
    }

    /// <summary>
    /// Crea un producto. Solo pueden hacerlo los usuarios con rol Proveedor o Administrador.
    /// </summary>
    public bool CrearProducto(int idSolicitante, Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        _autorizacionService.ValidarRol(idSolicitante, RolUsuario.Proveedor, RolUsuario.Administrador);

        if (_productoDAO.Existe(producto.IdProducto))
        {
            throw new InvalidOperationException($"Ya existe un producto con el ID {producto.IdProducto}.");
        }

        return _productoDAO.Crear(producto);
    }

    /// <summary>
    /// Elimina un producto del sistema. Solo pueden hacerlo los usuarios con rol Proveedor o Administrador,
    /// y solo si el producto no está siendo ofertado por ningún proveedor.
    /// </summary>
    /// <param name="idSolicitante">Id del usuario que solicita la eliminación.</param>
    /// <param name="idProducto"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public bool EliminarProducto(int idSolicitante, int idProducto)
    {
        _autorizacionService.ValidarRol(idSolicitante, RolUsuario.Proveedor, RolUsuario.Administrador);

        if (!_productoDAO.Existe(idProducto))
        {
            throw new KeyNotFoundException($"No existe un producto con el ID {idProducto}.");
        }

        if (_productoOfertadoDAO.ExistePorProducto(idProducto))
        {
            return false; // retorna false si el producto está siendo ofertado por algún proveedor
        }

        return _productoDAO.Eliminar(idProducto);
    }

    public Producto ConsultarProducto(int idProducto)
    {
        if (!_productoDAO.Existe(idProducto))
        {
            throw new KeyNotFoundException($"No existe un producto con el ID {idProducto}.");
        }

        return _productoDAO.ConsultarId(idProducto);
    }
}