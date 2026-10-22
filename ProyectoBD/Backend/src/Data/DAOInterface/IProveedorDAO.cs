using System;
using System.Collections.Generic;

/// </summary>
/// Interfaz para el acceso a datos de proveedores.
///Define los métodos para realizar operaciones CRUD sobre los proveedores.
///Los metodos retornan true si la operacion fue exitosa y false si no lo fue.
/// </summary>

public interface IProveedorDAO
{
    bool Crear(Proveedor proveedor);
    Proveedor ConsultarId(int idProveedor);

    bool Actualizar(Proveedor proveedor);

    bool Eliminar(int idProveedor);

    /// <summary>
    /// Verifica si un proveedor existe por su ID
    /// </summary>

    bool Existe(int idProveedor);
}