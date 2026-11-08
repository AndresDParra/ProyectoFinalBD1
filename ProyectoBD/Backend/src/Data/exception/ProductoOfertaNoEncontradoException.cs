public class ProductoOfertaNoEncontradoException : ProductoOfertadoDaoException
{
    public ProductoOfertaNoEncontradoException(int idProducto)
        : base($"No existe un producto con el ID {idProducto}.")
    {
    }
}
