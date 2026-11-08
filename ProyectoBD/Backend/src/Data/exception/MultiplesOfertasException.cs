public class MultiplesOfertasException : ProductoOfertadoDaoException
{
    public MultiplesOfertasException(int idProducto)
        : base($"El producto {idProducto} tiene varias ofertas; la interfaz requiere especificar el proveedor.")
    {
    }
}
