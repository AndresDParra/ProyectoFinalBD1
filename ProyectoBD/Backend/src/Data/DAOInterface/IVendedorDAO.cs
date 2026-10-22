using System;
using System.Collections.Generic;

/// </summary>
/// Interfaz para el acceso a datos de proveedores.
///Define los métodos para realizar operaciones CRUD sobre los proveedores.
///Los metodos retornan true si la operacion fue exitosa y false si no lo fue.
/// </summary>

public interface IVendedorDAO
{
    bool Crear(Vendedor vendedor);
    Vendedor ConsultarId(int idVendedor);

    bool Actualizar(Vendedor vendedor);

    bool Eliminar(int idVendedor);

    /// <summary>
    /// Verifica si un vendedor existe por su ID
    /// </summary>
    /// 
    bool Existe(int idVendedor);
}