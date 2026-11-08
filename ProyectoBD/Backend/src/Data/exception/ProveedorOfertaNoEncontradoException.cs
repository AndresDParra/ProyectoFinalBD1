public class ProveedorOfertaNoEncontradoException : ProductoOfertadoDaoException
{
    public ProveedorOfertaNoEncontradoException(int idProveedor)
        : base($"No existe un proveedor válido con el ID {idProveedor}.")
    {
    }
}
