using System;
using System.Collections.Generic;

/// </summary>
/// Interfaz para el acceso a datos de proveedores.
///Define los métodos para realizar operaciones CRUD sobre los proveedores.
///Los metodos retornan true si la operacion fue exitosa y false si no lo fue.
/// </summary>

public interface IProductoDAO
{
    bool Crear(Producto producto);
    Producto ConsultarId(int idProducto);

    bool Actualizar(Producto producto);

    bool Eliminar(int idProducto);

    /// <summary>
    /// Verifica si un producto existe por su ID
    /// </summary>
    bool Existe(int idProducto);
}