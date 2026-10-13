using System;
using System.Collections.Generic;

/// <summary>
/// Clase ProductoDAO que implementa la interfaz IProductoDAO para realizar operaciones CRUD en la entidad Producto.
/// en cada metodo se define la logica para interactuar con la base de datos, como insertar,
/// consultar, actualizar y eliminar registros de productos.
/// </summary>

public class ProductoDAO : IProductoDAO
{
    public bool Crear(Producto producto)
    {
        // TODO: INSERT INTO producto ...
        throw new NotImplementedException();
    }

    public Producto ConsultarId(int idProducto)
    {
        // TODO: SELECT ... WHERE id = @id
        throw new NotImplementedException();
    }

    public bool Actualizar(Producto producto)
    {
        // TODO: UPDATE producto SET ... WHERE id = @id
        throw new NotImplementedException();
    }

    public bool Eliminar(int idProducto)
    {
        // TODO: DELETE FROM producto WHERE id = @id
        throw new NotImplementedException();
    }

    public bool Existe(string idProducto)
    {
        throw new NotImplementedException();
    }
}