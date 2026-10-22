
using System.Collections.Generic;

/// </summary>
/// Interfaz para el acceso a datos de los productos ofertados.
///Define los métodos para realizar operaciones CRUD sobre los productos ofertados.
/// </summary>


public interface IProductoOfertadoDAO
{
    bool Crear(ProductoOfertado ofertado);
    bool ActualizarPrecio(int idProveedor, int idProducto, decimal nuevoPrecio);
    bool ExistePorProducto(int idProducto);

    List<ProductoOfertado> ConsultarIdProveedor(int idProveedor);
    ProductoOfertado ConsultarIdProducto(int idProducto);
    bool Eliminar(int idProveedor, int idProducto);
}
