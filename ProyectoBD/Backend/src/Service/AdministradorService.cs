/// <summary>
/// Representa un servicio para gestionar las operaciones relacionadas con los administradores del sistema.
/// todos las actciones que puede realizar el administrador se encuentran en este servicio,
/// como crear vendedores, proveedores y eliminar productos.
/// </summary>

public class AdministradorService
{
    private readonly AdministradorDAO _administradorDAO;
    private readonly VendedorDAO _vendedorDAO;

    private readonly ProveedorDAO _proveedorDAO;

    private readonly ProductoDAO _productoDAO;

    public AdministradorService(AdministradorDAO administradorDAO, VendedorDAO vendedorDAO, ProveedorDAO proveedorDAO, ProductoDAO productoDAO)
    {
        _administradorDAO = administradorDAO;
        _vendedorDAO = vendedorDAO;
        _proveedorDAO = proveedorDAO;
        _productoDAO = productoDAO;
    }

    /// <summary>
    /// actualiza la informacion del administrador en la base de datos, 
    /// si el administrador no existe retorna false
    /// </summary>
    /// <param name="administrador"></param> referencia al admnistrador actual 
    /// <returns></returns>

    public bool ActualizarAdministrador(Administrador administrador)
    {
        if (!_administradorDAO.Existe(administrador.IdAdministrador))
        {
            return false; // El administrador no existe
        }

        return _administradorDAO.Actualizar(administrador);
    }


    // operaciones para del administrador sobre los vendedores

    public bool CrearNuevoVendedor(Vendedor vendedor)
    {
        if (_vendedorDAO.Existe(vendedor.IdVendedor))
        {
            return false; // El vendedor ya existe
        }

        return _vendedorDAO.Crear(vendedor);
    }

    public bool EliminarVendedor(int idVendedor)
    {
        if (!_vendedorDAO.Existe(idVendedor))
        {
            return false; // El vendedor no existe
        }

        return _vendedorDAO.Eliminar(idVendedor);
    }

    public bool ActualizarVendedor(Vendedor vendedor)
    {
        if (!_vendedorDAO.Existe(vendedor.IdVendedor))
        {
            return false; // El vendedor no existe
        }

        return _vendedorDAO.Actualizar(vendedor);
    }

    public Vendedor ObtenerVendedorPorId(int idVendedor)
    {
        if (!_vendedorDAO.Existe(idVendedor))
        {
            throw new KeyNotFoundException($"No existe un vendedor con el ID {idVendedor}.");
        }

        return _vendedorDAO.ConsultarId(idVendedor);
    }

    // operaciones para del administrador sobre los proveedores

    public bool CrearNuevoProveedor(Proveedor proveedor)
    {
        if (_proveedorDAO.Existe(proveedor.IdProveedor))
        {
            return false; // El proveedor ya existe
        }

        return _proveedorDAO.Crear(proveedor);
    }

    public bool EliminarProveedor(int idProveedor)
    {
        if (!_proveedorDAO.Existe(idProveedor))
        {
            return false; // El proveedor no existe
        }

        return _proveedorDAO.Eliminar(idProveedor);
    }

    public bool ActualizarProveedor(Proveedor proveedor)
    {
        if (!_proveedorDAO.Existe(proveedor.IdProveedor))
        {
            return false; // El proveedor no existe
        }

        return _proveedorDAO.Actualizar(proveedor);
    }

    public Proveedor ObtenerProveedorPorId(int idProveedor)
    {
        if (!_proveedorDAO.Existe(idProveedor))
        {
            throw new KeyNotFoundException($"No existe un proveedor con el ID {idProveedor}.");
        }

        return _proveedorDAO.ConsultarId(idProveedor);
    }

    // operaciones para del administrador sobre los productos

    public Producto ObtenerProductoPorId(int idProducto)
    {
        if (!_productoDAO.Existe(idProducto))
        {
            throw new KeyNotFoundException($"No existe un producto con el ID {idProducto}.");
        }

        return _productoDAO.ConsultarId(idProducto);
    }
}
