using System;
using System.Collections.Generic;

/// <summary>
/// Interfaz para el acceso a datos de usuarios.
/// Define los métodos para realizar operaciones CRUD sobre los usuarios.
/// ademas de diferentes consultas personalizadas sobre el sistema
/// </summary>
/// 
public interface IUsuarioDAO
{

    /// <summary>
    /// metodos para consultar usuario apartir de sus diferentes atributos
    /// </summary>

    Usuario ObtenerUsuarioPorId(int id);
    Usuario ObtenerUsuarioPorCedula(string cedula);
    Usuario ObtenerUsuarioPorCorreo(string correo);
    RolUsuario? ObtenerRolUsuarioPorId(int id); // se habilita el nulleable con '?' para poder retornar null en caso de que no se encuentre el rol del usuario

    /// <summary>
    /// metodos para consultar listas de usuarios apartir de sus diferentes atributos
    /// </summary>

    List<Usuario> ObtenerUsuarioPorIdTipo(string tipo);
    List<Usuario> ObtenerUsuarioPorNombre(string nombre);

    /// <summary>
    /// metodos para consultar lista de proveedores apartir de sus diferentes atributos
    /// </summary>

    List<Usuario> ObtenerProveedorPorCiudad(string ciudad);
    List<Usuario> ObtenerProveedorPorNombreEmpresa(string nombreEmpresa);

    /// <summary>
    ///  metodos para gestionar Usuario
    /// </summary>

    bool CrearUsuario(Usuario usuario);
    bool ActualizarUsuario(Usuario usuario);
    bool EliminarUsuario(int id);
}