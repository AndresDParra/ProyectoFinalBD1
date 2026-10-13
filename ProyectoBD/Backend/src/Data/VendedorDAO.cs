using System;
using System.Collections.Generic;

/// <summary>
/// Clase VendedorDAO que implementa la interfaz IVendedorDAO para realizar operaciones CRUD en la entidad Vendedor.
/// en cada metodo se define la logica para interactuar con la base de datos, como insertar,
/// consultar, actualizar y eliminar registros de vendedores.
/// </summary>

public class VendedorDAO : IVendedorDAO
{
    public bool Crear(Vendedor proveedor)
    {
        // TODO: INSERT INTO proveedor ...
        throw new NotImplementedException();
    }

    public Vendedor ConsultarId(int idVendedor)
    {
        // TODO: SELECT ... WHERE id = @id
        throw new NotImplementedException();
    }

    public bool Actualizar(Vendedor vendedor)
    {
        // TODO: UPDATE vendedor SET ... WHERE id = @id
        throw new NotImplementedException();
    }

    public bool Eliminar(int idVendedor)
    {
        // TODO: DELETE FROM vendedor WHERE id = @id
        throw new NotImplementedException();
    }

    public bool Existe(string idVendedor)
    {
        throw new NotImplementedException();
    }
}