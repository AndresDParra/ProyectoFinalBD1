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
        u.idusuario,
        u.nombre,
        u.direccion,
        u.telefono,
        u.cedula,
        u.correo,
        u.fechanacimiento,
        u.genero,
        u.rolusuario,
        u.nombreempresa,
        u.ciudad
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
        int idUsuario = lector.GetInt32(lector.GetOrdinal("idusuario"));
        int valorRol = lector.GetInt32(lector.GetOrdinal("rolusuario"));

        if (!Enum.IsDefined(typeof(RolUsuario), valorRol))
            throw new RolUsuarioInvalidoException(idUsuario, valorRol);

        // Columnas opcionales: solo se usan cuando el rol es Proveedor. Para los demás roles
        // se ignoran aunque la fila las tenga llenas, porque Usuario no las admite.
        string? nombreEmpresa = null;
        string? ciudad = null;
        if (valorRol == (int)RolUsuario.Proveedor)
        {
            int posNombreEmpresa = lector.GetOrdinal("nombreempresa");
            int posCiudad = lector.GetOrdinal("ciudad");
            nombreEmpresa = lector.IsDBNull(posNombreEmpresa) ? null : lector.GetString(posNombreEmpresa);
            ciudad = lector.IsDBNull(posCiudad) ? null : lector.GetString(posCiudad);
        }

        return CrearUsuario(
            idUsuario,
            lector.GetString(lector.GetOrdinal("nombre")),
            lector.GetString(lector.GetOrdinal("direccion")),
            lector.GetString(lector.GetOrdinal("telefono")),
            lector.GetString(lector.GetOrdinal("cedula")),
            lector.GetString(lector.GetOrdinal("correo")),
            DateOnly.FromDateTime(lector.GetDateTime(lector.GetOrdinal("fechanacimiento"))),
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
        const string sql = $"SELECT {ColumnasUsuario} FROM usuario u WHERE u.idusuario = @idUsuario;";

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
        const string sql = $"SELECT {ColumnasUsuario} FROM usuario u WHERE u.correo = @correo;";

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
        const string sql = "SELECT rolusuario FROM usuario WHERE idusuario = @idUsuario;";

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

        const string sql = $"SELECT {ColumnasUsuario} FROM usuario u WHERE u.rolusuario = @idTipoUsuario ORDER BY u.idusuario;";
        return ObtenerLista(sql, comando => comando.Parameters.AddWithValue("@idTipoUsuario", idTipoUsuario));
    }

    /// <summary>
    /// metodos para consultar lista de proveedores apartir de sus diferentes atributos
    /// </summary>

    public List<Usuario> ObtenerProveedorPorCiudad(string ciudad)
    {
        string sql = $"SELECT {ColumnasUsuario} FROM usuario u WHERE u.rolusuario = {(int)RolUsuario.Proveedor} AND u.ciudad = @ciudad ORDER BY u.idusuario;";
        return ObtenerLista(sql, comando => comando.Parameters.AddWithValue("@ciudad", ciudad));
    }

    public List<Usuario> ObtenerProveedorPorNombreEmpresa(string nombreEmpresa)
    {
        string sql = $"SELECT {ColumnasUsuario} FROM usuario u WHERE u.rolusuario = {(int)RolUsuario.Proveedor} AND u.nombreempresa = @nombreEmpresa ORDER BY u.idusuario;";
        return ObtenerLista(sql, comando => comando.Parameters.AddWithValue("@nombreEmpresa", nombreEmpresa));
    }

    /// <summary>
    ///  metodos para gestionar Usuario
    /// </summary>

    public bool CrearUsuario(Usuario usuario, string contrasenaHash)
    {
        // nombreempresa y ciudad viven en la tabla usuario; son NULL salvo para los proveedores
        const string sql = """
            INSERT INTO usuario
                (idusuario, nombre, contrasena, direccion, telefono, cedula, correo,
                 fechanacimiento, genero, rolusuario, nombreempresa, ciudad)
            VALUES
                (@idUsuario, @nombre, @contrasena, @direccion, @telefono, @cedula, @correo,
                 @fechaNacimiento, @genero, @idTipoUsuario, @nombreEmpresa, @ciudad);
            """;

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@idUsuario", usuario.IdUsuario);
            comando.Parameters.AddWithValue("@nombre", usuario.Nombre);
            comando.Parameters.AddWithValue("@contrasena", contrasenaHash); // se almacena el hash de la contraseña, no la contraseña en texto plano
            comando.Parameters.AddWithValue("@direccion", usuario.Direccion);
            comando.Parameters.AddWithValue("@telefono", usuario.Telefono);
            comando.Parameters.AddWithValue("@cedula", usuario.Cedula);
            comando.Parameters.AddWithValue("@correo", usuario.Correo);
            comando.Parameters.AddWithValue("@fechaNacimiento", usuario.FechaNacimiento.ToDateTime(TimeOnly.MinValue));
            comando.Parameters.AddWithValue("@genero", usuario.Genero);
            comando.Parameters.AddWithValue("@idTipoUsuario", usuario.IdTipoUsuario);
            comando.Parameters.AddWithValue("@nombreEmpresa", (object?)usuario.NombreEmpresa ?? DBNull.Value);
            comando.Parameters.AddWithValue("@ciudad", (object?)usuario.Ciudad ?? DBNull.Value);

            return comando.ExecuteNonQuery() == 1;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }

    public bool ActualizarUsuario(Usuario usuario)
    {
        // empresa y ciudad solo se modifican para proveedores; en los demás roles se conservan tal cual
        string sql = usuario.Rol == RolUsuario.Proveedor
            ? """
              UPDATE usuario
              SET nombre = @nombre,
                  direccion = @direccion,
                  telefono = @telefono,
                  correo = @correo,
                  nombreempresa = @nombreEmpresa,
                  ciudad = @ciudad
              WHERE idusuario = @idUsuario;
              """
            : """
              UPDATE usuario
              SET nombre = @nombre,
                  direccion = @direccion,
                  telefono = @telefono,
                  correo = @correo
              WHERE idusuario = @idUsuario;
              """;

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@nombre", usuario.Nombre);
            comando.Parameters.AddWithValue("@direccion", usuario.Direccion);
            comando.Parameters.AddWithValue("@telefono", usuario.Telefono);
            comando.Parameters.AddWithValue("@correo", usuario.Correo);
            comando.Parameters.AddWithValue("@idUsuario", usuario.IdUsuario);

            if (usuario.Rol == RolUsuario.Proveedor)
            {
                comando.Parameters.AddWithValue("@nombreEmpresa", usuario.NombreEmpresa);
                comando.Parameters.AddWithValue("@ciudad", usuario.Ciudad);
            }

            return comando.ExecuteNonQuery() == 1;
        }
        catch (MySqlException exception)
        {
            throw new ErrorDeBaseDeDatos(exception);
        }
    }

    public bool EliminarUsuario(int id)
    {
        const string sql = "DELETE FROM usuario WHERE idusuario = @idUsuario;";

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
        const string sql = "SELECT COUNT(*) FROM usuario WHERE idusuario = @idUsuario;";

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
            "UPDATE usuario SET contrasena = @hash WHERE idusuario = @id", conexion);

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
        string sql = $"SELECT u.contrasena, {ColumnasUsuario} FROM usuario u WHERE u.correo = @correo;";

        try
        {
            using var conexion = Conexion.Obtener();
            conexion.Open();
            using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@correo", correo);

            using var lector = comando.ExecuteReader();
            if (!lector.Read()) return null;

            // un usuario sin contraseña registrada (columna NULL) no puede iniciar sesión
            int posHash = lector.GetOrdinal("contrasena");
            if (lector.IsDBNull(posHash)) return null;

            string hash = lector.GetString(posHash);
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
                "SELECT contrasena FROM usuario WHERE idusuario = @id", conexion);
            comando.Parameters.AddWithValue("@id", idUsuario);

            return comando.ExecuteScalar() as string;
        }
        catch (MySqlException ex)
        {
            throw new ErrorDeBaseDeDatos(ex);
        }
    }
}