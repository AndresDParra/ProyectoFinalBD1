using System;
using System.Collections.Generic;
using System.Reflection;
using MySql.Data.MySqlClient;
public class UsuarioDAO : IUsuarioDAO
{
    private const string ColumnasUsuario = """
        u.idUsuario,
        u.nombre,
        u.direccion,
        u.telefono,
        u.cedula,
        u.correo,
        u.fechaNacimiento,
        u.genero,
        u.idTipoUsuario,
        p.nombreempresa,
        p.ciudad
        """;

    private const string JoinProveedor = """
        LEFT JOIN Proveedor p ON p.idproveedor = u.idUsuario
        """;

    private static Usuario CrearUsuario(
        int idUsuario,
        string nombre,
        string direccion,
        string telefono,
        string cedula,
        string correo,
        DateOnly fechaNacimiento,
        string genero,
        RolUsuario rol,
        string? nombreEmpresa,
        string? ciudad)
    {
        object? usuario = Activator.CreateInstance(
            typeof(Usuario),
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            args:
            [
                idUsuario,
                nombre,
                direccion,
                telefono,
                cedula,
                correo,
                fechaNacimiento,
                genero,
                rol,
                nombreEmpresa,
                ciudad
            ],
            culture: null);

        return usuario as Usuario
            ?? throw new InvalidOperationException("No se pudo crear el usuario desde la base de datos.");
    }

    private static Usuario MapearUsuario(MySqlDataReader lector)
    {
        int idUsuario = lector.GetInt32("idUsuario");
        int valorRol = lector.GetInt32("idTipoUsuario");

        if (!Enum.IsDefined(typeof(RolUsuario), valorRol))
            throw new RolUsuarioInvalidoException(idUsuario, valorRol);

        string? nombreEmpresa = lector.IsDBNull(9) ? null : lector.GetString(9);
        string? ciudad = lector.IsDBNull(10) ? null : lector.GetString(10);

        return CrearUsuario(
            idUsuario,
            lector.GetString("nombre"),
            lector.GetString("direccion"),
            lector.GetString("telefono"),
            lector.GetString("cedula"),
            lector.GetString("correo"),
            DateOnly.FromDateTime(lector.GetDateTime("fechaNacimiento")),
            lector.GetString("genero"),
            (RolUsuario)valorRol,
            nombreEmpresa,
            ciudad);
    }

    /// <summary>
    /// metodos para consultar usuario apartir de sus diferentes atributos
    /// </summary>

    public Usuario ObtenerUsuarioPorId(int id)
    {
        const string sql = $"SELECT {ColumnasUsuario} FROM Usuario u {JoinProveedor} WHERE u.idUsuario = @idUsuario;";

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@idUsuario", id);

            using var lector = comando.ExecuteReader();
            if (!lector.Read())
                throw new UsuarioNoEncontradoException(id);

            return MapearUsuario(lector);
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }

    public Usuario ObtenerUsuarioPorCorreo(string correo)
    {
        const string sql = $"SELECT {ColumnasUsuario} FROM Usuario u {JoinProveedor} WHERE u.correo = @correo;";

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@correo", correo);

            using var lector = comando.ExecuteReader();
            if (!lector.Read())
                throw new CorreoUsuarioNoEncontradoException(correo);

            return MapearUsuario(lector);
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }
    public RolUsuario? ObtenerRolUsuarioPorId(int idUsuario)
    {
        const string sql = "SELECT idTipoUsuario FROM Usuario WHERE idUsuario = @idUsuario;";

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@idUsuario", idUsuario);

            object? resultado = comando.ExecuteScalar();
            if (resultado is null || resultado == DBNull.Value)
                return null;

            int valorRol = Convert.ToInt32(resultado);
            if (!Enum.IsDefined(typeof(RolUsuario), valorRol))
                throw new RolUsuarioInvalidoException(idUsuario, valorRol);

            return (RolUsuario)valorRol;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }


    /// <summary>
    /// metodos para consultar listas de usuarios apartir de sus diferentes atributos
    /// </summary>

    public List<Usuario> ObtenerUsuarioPorIdTipo(string tipo)
    {
        int idTipoUsuario;
        if (int.TryParse(tipo, out int tipoNumerico))
        {
            if (!Enum.IsDefined(typeof(RolUsuario), tipoNumerico))
                throw new ArgumentException("El tipo de usuario no es válido.", nameof(tipo));

            idTipoUsuario = tipoNumerico;
        }
        else if (Enum.TryParse<RolUsuario>(tipo, true, out var rol) && Enum.IsDefined(rol))
        {
            idTipoUsuario = (int)rol;
        }
        else
        {
            throw new ArgumentException("El tipo de usuario no es válido.", nameof(tipo));
        }

        const string sql = $"SELECT {ColumnasUsuario} FROM Usuario u {JoinProveedor} WHERE u.idTipoUsuario = @idTipoUsuario ORDER BY u.idUsuario;";
        return ObtenerLista(sql, comando => comando.Parameters.AddWithValue("@idTipoUsuario", idTipoUsuario));
    }

    /// <summary>
    /// metodos para consultar lista de proveedores apartir de sus diferentes atributos
    /// </summary>

    public List<Usuario> ObtenerProveedorPorCiudad(string ciudad)
    {
        const string sql = $"SELECT {ColumnasUsuario} FROM Usuario u INNER JOIN Proveedor p ON p.idproveedor = u.idUsuario WHERE p.ciudad = @ciudad ORDER BY u.idUsuario;";
        return ObtenerLista(sql, comando => comando.Parameters.AddWithValue("@ciudad", ciudad));
    }

    public List<Usuario> ObtenerProveedorPorNombreEmpresa(string nombreEmpresa)
    {
        const string sql = $"SELECT {ColumnasUsuario} FROM Usuario u INNER JOIN Proveedor p ON p.idproveedor = u.idUsuario WHERE p.nombreempresa = @nombreEmpresa ORDER BY u.idUsuario;";
        return ObtenerLista(sql, comando => comando.Parameters.AddWithValue("@nombreEmpresa", nombreEmpresa));
    }

    private static List<Usuario> ObtenerLista(string sql, Action<MySqlCommand> agregarParametros)
    {
        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(sql, conexion);
            agregarParametros(comando);

            using var lector = comando.ExecuteReader();
            var usuarios = new List<Usuario>();
            while (lector.Read())
                usuarios.Add(MapearUsuario(lector));

            return usuarios;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }

    /// <summary>
    ///  metodos para gestionar Usuario
    /// </summary>

    public bool CrearUsuario(Usuario usuario)
    {
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

    /// metodo para validar la existencia de un usuario en el sistema
    public bool ExisteUsuario(int id)
    {
        const string sql = "SELECT COUNT(*) FROM Usuario WHERE idUsuario = @idUsuario;";

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();

            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@idUsuario", id);

            // convierte el resultado en un entero y verifica si es mayor a 0, lo que indica que el usuario existe

            object resultado = comando.ExecuteScalar()!;
            int cantidad = Convert.ToInt32(resultado);

            return cantidad > 0;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }
}