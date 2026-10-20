using System;
using System.Collections.Generic;

/// </summary>
/// Interfaz para el acceso a datos de proveedores.
///Define los métodos para realizar operaciones CRUD sobre los proveedores.
///Los metodos retornan true si la operacion fue exitosa y false si no lo fue.
/// </summary>

public interface IAdministradorDAO
{
    bool Crear(Administrador administrador);
    Administrador ConsultarId(int idAdministrador);

    bool Actualizar(Administrador administrador);
}