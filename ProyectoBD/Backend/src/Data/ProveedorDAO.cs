using System;
using System.Collections.Generic;

/// <summary>
/// Clase ProveedorDAO que implementa la interfaz IProveedorDAO para realizar operaciones CRUD en la entidad Proveedor.
/// en cada metodo se define la logica para interactuar con la base de datos, como insertar,
/// consultar, actualizar y eliminar registros de proveedores.
/// </summary>

public class ProveedorDAO : IProveedorDAO
{
    public bool Crear(Proveedor proveedor)
    {
        // TODO: INSERT INTO proveedor ...
        throw new NotImplementedException();
    }

    public Proveedor ConsultarId(int id)
    {
        // TODO: SELECT ... WHERE id = @id
        throw new NotImplementedException();
    }

    public bool Actualizar(Proveedor proveedor)
    {
        // TODO: UPDATE proveedor SET ... WHERE id = @id
        throw new NotImplementedException();
    }

    public bool Eliminar(int id)
    {
        // TODO: DELETE FROM proveedor WHERE id = @id
        throw new NotImplementedException();
    }

    public bool Existe(string idProveedor)
    {
        throw new NotImplementedException();
    }
}