using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
public class UsuarioDAO : IUsuarioDAO
{

    /// <summary>
    /// metodos auxiliares que facilitan la gestion de usuarios y validan su estado antes de realizar operaciones CRUD
    /// </summary>

    /// <summary>
    /// Valida que el usuario pueda ser gestionado mediante el CRUD habitual.
    /// </summary>
    private static void ValidarUsuarioParaCrud(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        usuario.ValidarEstado();

        if (usuario.Rol == RolUsuario.Administrador)
            throw new OperacionUsuarioNoPermitidaException("El administrador no se gestiona mediante el CRUD habitual.");

    }

    /// <summary>
    /// Obtiene y valida el rol de un usuario bloqueando su fila durante la transaccion.
    /// </summary>
    private static RolUsuario ObtenerRolBloqueado(MySqlConnection conexion, MySqlTransaction transaccion, int idUsuario)
    {
        const string sql = "SELECT idTipoUsuario FROM Usuario WHERE idUsuario = @idUsuario FOR UPDATE;";

        using var comando = new MySqlCommand(sql, conexion, transaccion);
        comando.Parameters.AddWithValue("@idUsuario", idUsuario);

        object? resultado = comando.ExecuteScalar();
        if (resultado is null || resultado == DBNull.Value)
            throw new UsuarioNoEncontradoException(idUsuario);

        int valorRol = Convert.ToInt32(resultado);
        if (!Enum.IsDefined(typeof(RolUsuario), valorRol))
            throw new RolUsuarioInvalidoException(idUsuario, valorRol);

        return (RolUsuario)valorRol;
    }

    /// <summary>
    /// Convierte un valor de texto de MySQL en una cadena nullable y elimina sus espacios externos.
    /// </summary>
    private static string? LeerTextoNullable(MySqlDataReader reader, string columna)
    {
        object valor = reader[columna];
        return valor == DBNull.Value ? null : ((string)valor).Trim();
    }

    /// <summary>
    /// metodos para consultar usuario apartir de sus diferentes atributos
    /// </summary>


    public Usuario ObtenerUsuarioPorId(int id) { return null; }


    public Usuario ObtenerUsuarioPorCedula(string cedula) { return null; }

    public Usuario ObtenerUsuarioPorCorreo(string correo) { return null; }
    public RolUsuario? ObtenerRolUsuarioPorId(int idUsuario) { return null; }


    /// <summary>
    /// metodos para consultar listas de usuarios apartir de sus diferentes atributos
    /// </summary>

    public List<Usuario> ObtenerUsuarioPorIdTipo(string tipo) { return null; }

    public List<Usuario> ObtenerUsuarioPorNombre(string nombre) { return null; }

    /// <summary>
    /// metodos para consultar lista de proveedores apartir de sus diferentes atributos
    /// </summary>

    public List<Usuario> ObtenerProveedorPorCiudad(string ciudad) { return null; }
    public List<Usuario> ObtenerProveedorPorNombreEmpresa(string nombreEmpresa) { return null; }

    /// <summary>
    ///  metodos para gestionar Usuario
    /// </summary>

    public bool CrearUsuario(Usuario usuario)
    {
        ValidarUsuarioParaCrud(usuario);

        const string sqlUsuario = """
            INSERT INTO Usuario
                (idUsuario, nombre, direccion, telefono, cedula, correo,
                 fechaNacimiento, genero, idTipoUsuario)
            VALUES
                (@idUsuario, @nombre, @direccion, @telefono, @cedula, @correo,
                 @fechaNacimiento, @genero, @idTipoUsuario);
            """;

        const string sqlProveedor = """
            INSERT INTO Proveedor (idproveedor, nombreempresa, ciudad)
            VALUES (@idProveedor, @nombreEmpresa, @ciudad);
            """;

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();

            using var transaccion = conexion.BeginTransaction();
            using var comandoUsuario = new MySqlCommand(sqlUsuario, conexion, transaccion);

            comandoUsuario.Parameters.AddWithValue("@idUsuario", usuario.IdUsuario);
            comandoUsuario.Parameters.AddWithValue("@nombre", usuario.Nombre);
            comandoUsuario.Parameters.AddWithValue("@direccion", usuario.Direccion);
            comandoUsuario.Parameters.AddWithValue("@telefono", usuario.Telefono);
            comandoUsuario.Parameters.AddWithValue("@cedula", usuario.Cedula);
            comandoUsuario.Parameters.AddWithValue("@correo", usuario.Correo);
            comandoUsuario.Parameters.AddWithValue("@fechaNacimiento", usuario.FechaNacimiento.ToDateTime(TimeOnly.MinValue));
            comandoUsuario.Parameters.AddWithValue("@genero", usuario.Genero);
            comandoUsuario.Parameters.AddWithValue("@idTipoUsuario", usuario.IdTipoUsuario);

            if (comandoUsuario.ExecuteNonQuery() != 1)
            {
                transaccion.Rollback();
                return false;
            }

            /// verifica si el usuario es proveedor agrega los valores especificos del proveedor a la tabla usuario
            if (usuario.Rol == RolUsuario.Proveedor)
            {
                using var comandoProveedor = new MySqlCommand(sqlProveedor, conexion, transaccion);
                comandoProveedor.Parameters.AddWithValue("@idProveedor", usuario.IdUsuario);
                comandoProveedor.Parameters.AddWithValue("@nombreEmpresa", usuario.NombreEmpresa);
                comandoProveedor.Parameters.AddWithValue("@ciudad", usuario.Ciudad);

                if (comandoProveedor.ExecuteNonQuery() != 1)
                {
                    transaccion.Rollback();
                    return false;
                }
            }

            transaccion.Commit();
            return true;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }

    public bool ActualizarUsuario(Usuario usuario)
    {
        ValidarUsuarioParaCrud(usuario);

        const string sqlUsuario = """
            UPDATE Usuario
            SET nombre = @nombre,
                direccion = @direccion,
                telefono = @telefono,
                correo = @correo
            WHERE idUsuario = @idUsuario;
            """;

        const string sqlProveedor = """
            UPDATE Proveedor
            SET nombreempresa = @nombreEmpresa,
                ciudad = @ciudad
            WHERE idproveedor = @idProveedor;
            """;

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var transaccion = conexion.BeginTransaction();

            RolUsuario rolPersistido = ObtenerRolBloqueado(conexion, transaccion, usuario.IdUsuario);
            if (rolPersistido == RolUsuario.Administrador || rolPersistido != usuario.Rol)
                throw new OperacionUsuarioNoPermitidaException("No se puede modificar el administrador ni cambiar el rol del usuario mediante este CRUD.");

            using var comando = new MySqlCommand(sqlUsuario, conexion, transaccion);

            comando.Parameters.AddWithValue("@nombre", usuario.Nombre);
            comando.Parameters.AddWithValue("@direccion", usuario.Direccion);
            comando.Parameters.AddWithValue("@telefono", usuario.Telefono);
            comando.Parameters.AddWithValue("@correo", usuario.Correo);
            comando.Parameters.AddWithValue("@idUsuario", usuario.IdUsuario);

            if (comando.ExecuteNonQuery() != 1)
            {
                transaccion.Rollback();
                return false;
            }

            if (usuario.Rol == RolUsuario.Proveedor)
            {
                using var comandoProveedor = new MySqlCommand(sqlProveedor, conexion, transaccion);
                comandoProveedor.Parameters.AddWithValue("@nombreEmpresa", usuario.NombreEmpresa);
                comandoProveedor.Parameters.AddWithValue("@ciudad", usuario.Ciudad);
                comandoProveedor.Parameters.AddWithValue("@idProveedor", usuario.IdUsuario);

                if (comandoProveedor.ExecuteNonQuery() != 1)
                {
                    transaccion.Rollback();
                    return false;
                }
            }

            transaccion.Commit();
            return true;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }

    public bool EliminarUsuario(int id)
    {
        const string sql = "DELETE FROM Usuario WHERE idUsuario = @idUsuario;";

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var transaccion = conexion.BeginTransaction();

            RolUsuario rolPersistido = ObtenerRolBloqueado(conexion, transaccion, id);
            if (rolPersistido == RolUsuario.Administrador)
                throw new OperacionUsuarioNoPermitidaException("El administrador no puede eliminarse mediante el CRUD habitual.");

            using var comando = new MySqlCommand(sql, conexion, transaccion);
            comando.Parameters.AddWithValue("@idUsuario", id);

            if (comando.ExecuteNonQuery() != 1)
            {
                transaccion.Rollback();
                return false;
            }

            transaccion.Commit();
            return true;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }
}