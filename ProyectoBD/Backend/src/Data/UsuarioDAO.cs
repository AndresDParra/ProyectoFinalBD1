using System;
using System.Collections.Generic;
using System.Reflection;
using MySql.Data.MySqlClient;
public class UsuarioDAO : IUsuarioDAO
{
    /// <summary>
    ///  atributos con el propocido de representar una cadena SQL que contiene las columnas de la tabla Usuario 
    /// que se necesitan para mapear un objeto Usuario, ademas de servir como apoyo para metodos de consultas
    /// </summary>

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

    /// <summary>
    /// metodos privados que sirven para facilitar la ejecucion de diferentes consultas SQL y mapear los resultados a objetos Usuario.
    /// se finalidad es evitar escribir multiples veces bloques de codigo repetitivo
    /// </summary>


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

    /// <summary>
    /// Convierte la fila actual del <see cref="MySqlDataReader"/> en la entidad
    /// <see cref="Usuario"/> que corresponde a su rol (Administrador, Proveedor o Vendedor).
    /// Lee las columnas por nombre, no por posición, para que no se rompa si cambia
    /// el orden de las columnas de la tabla. Las columnas propias del proveedor
    /// (<c>nombreEmpresa</c> y <c>ciudad</c>) pueden ser NULL en los demás roles.
    /// No avanza el lector: el método que lo llama debe haber ejecutado <c>Read()</c>.
    /// </summary>

    private static Usuario MapearUsuario(MySqlDataReader lector)
    {
        int idUsuario = lector.GetInt32(lector.GetOrdinal("idUsuario"));
        int valorRol = lector.GetInt32(lector.GetOrdinal("idTipoUsuario"));

        if (!Enum.IsDefined(typeof(RolUsuario), valorRol))
            throw new RolUsuarioInvalidoException(idUsuario, valorRol);

        // Columnas opcionales: solo tienen valor cuando el rol es Proveedor
        int posNombreEmpresa = lector.GetOrdinal("nombreEmpresa");
        int posCiudad = lector.GetOrdinal("ciudad");

        string? nombreEmpresa = lector.IsDBNull(posNombreEmpresa) ? null : lector.GetString(posNombreEmpresa);
        string? ciudad = lector.IsDBNull(posCiudad) ? null : lector.GetString(posCiudad);

        return CrearUsuario(
            idUsuario,
            lector.GetString(lector.GetOrdinal("nombre")),
            lector.GetString(lector.GetOrdinal("direccion")),
            lector.GetString(lector.GetOrdinal("telefono")),
            lector.GetString(lector.GetOrdinal("cedula")),
            lector.GetString(lector.GetOrdinal("correo")),
            DateOnly.FromDateTime(lector.GetDateTime(lector.GetOrdinal("fechaNacimiento"))),
            lector.GetString(lector.GetOrdinal("genero")),
            (RolUsuario)valorRol,
            nombreEmpresa,
            ciudad);
    }

    /// <summary>
    /// este metodo tiene como objetivo ejerutar un SELECT que devuelve varios usuarios y los mapea a una lista de objetos Usuario.
    /// Se utiliza para consultas que devuelven múltiples resultados, como obtener todos los usuarios de un tipo específico 
    /// o todos los proveedores de una ciudad determinada.
    /// </summary>

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

    /// <summary>
    ///  metodos para gestionar Usuario
    /// </summary>

    public bool CrearUsuario(Usuario usuario, string contrasenaHash)
    {
        const string sqlUsuario = """
            INSERT INTO Usuario
                (idUsuario, nombre, contrasena, direccion, telefono, cedula, correo,
                 fechaNacimiento, genero, idTipoUsuario)
            VALUES
                (@idUsuario, @nombre, @contrasena, @direccion, @telefono, @cedula, @correo,
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
            comandoUsuario.Parameters.AddWithValue("@contrasena", contrasenaHash); // se almacena el hash de la contraseña, no la contraseña en texto plano
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

    /// <summary>
    /// metodos para gestionar las credenciales de un usuario
    /// </summary>

    public void ActualizarContrasena(int idUsuario, string nuevoHash)
    {
        using var conexion = Conexion.Obtener();
        using var comando = new MySqlCommand(
            "UPDATE Usuario SET contrasena = @hash WHERE idUsuario = @id", conexion);

        comando.Parameters.AddWithValue("@hash", nuevoHash);
        comando.Parameters.AddWithValue("@id", idUsuario);
        conexion.Open();
        comando.ExecuteNonQuery();
    }

    /// <summary>
    /// Devuelve el usuario y su hash de contraseña en una sola consulta.
    /// Solo lo usa el login. Devuelve null si el correo no existe.
    /// </summary>
    public (Usuario Usuario, string Hash)? ObtenerCredencialesPorCorreo(string correo)
    {
        string sql = $"SELECT u.contrasena, {ColumnasUsuario} FROM Usuario u {JoinProveedor} WHERE u.correo = @correo;";

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@correo", correo);

            using var lector = comando.ExecuteReader();
            if (!lector.Read()) return null;

            string hash = lector.GetString(lector.GetOrdinal("contrasena"));
            Usuario usuario = MapearUsuario(lector);
            return (usuario, hash);
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }

    /// <summary>
    /// Devuelve solo el hash del usuario, o null si no existe.
    /// </summary>
    public string? ObtenerHashPorId(int idUsuario)
    {
        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(
                "SELECT contrasena FROM Usuario WHERE idUsuario = @id", conexion);
            comando.Parameters.AddWithValue("@id", idUsuario);

            return comando.ExecuteScalar() as string;
        }
        catch (MySqlException ex)
        {
            throw new ErrorDeBaseDeDatos(ex);
        }
    }
}