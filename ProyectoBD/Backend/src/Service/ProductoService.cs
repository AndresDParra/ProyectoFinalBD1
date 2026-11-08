public class ProductoService
{
    private readonly IProductoDAO _productoDAO;
    private readonly IProductoOfertadoDAO _productoOfertadoDAO;

    public ProductoService(IProductoDAO productoDAO, IProductoOfertadoDAO productoOfertadoDAO)
    {
        _productoDAO = productoDAO;
        _productoOfertadoDAO = productoOfertadoDAO;
    }

    public bool CrearProducto(Producto producto)
    {
        ArgumentNullException.ThrowIfNull(producto);

        if (_productoDAO.Existe(producto.IdProducto))
        {
            throw new InvalidOperationException($"Ya existe un producto con el ID {producto.IdProducto}.");
        }

        return _productoDAO.Crear(producto);
    }

    /// <summary>
    /// Elimina un producto del sistema. Solo se puede eliminar un producto si no está siendo ofertado por ningún proveedor.
    /// </summary>
    /// <param name="idProducto"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public bool EliminarProducto(int idProducto)
    {
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